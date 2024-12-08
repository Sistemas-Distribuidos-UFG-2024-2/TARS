#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <amqp.h>
#include <amqp_tcp_socket.h>
#include <unistd.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <netdb.h>

#define PORT 5672
#define QUEUE_NAME "acceleration_queue"
#define FILE_PATH "acceleration_values.txt"

typedef struct {
    float acceleration;
} Data;

/**
 * Intervalo normal de acceleração de uma espaçonave que está orbitando a Terra em velocidade constante:
 * -1 a 1 micrômetros por segundo ao quadrado
 */
 
const char* get_hostname() {
    const char* hostname = getenv("RABBITMQ_HOSTNAME");
    if (hostname == NULL) {
        printf("Error: HOSTNAME environment variable not set\n");
        exit(1);
    }

    return hostname;
}

const char* get_socket_server_hostname() {
    const char* hostname = getenv("SOCKET_SERVER_HOSTNAME");
    if (hostname == NULL) {
        printf("Error: SOCKET_SERVER_HOSTNAME environment variable not set\n");
        exit(1);
    }

    return hostname;
}

int get_socket_server_port() {
    const char* port_str = getenv("SOCKET_SERVER_PORT");
    if (port_str == NULL) {
        printf("Error: SOCKET_SERVER_PORT environment variable not set\n");
        exit(1);
    }
    
    return atoi(port_str);
}

amqp_connection_state_t connect_rabbitmq() {
    amqp_connection_state_t conn;
    int attempt = 0;
    const int max_attempts = 5;

    while(1) {
        conn = amqp_new_connection();
        amqp_socket_t *socket = amqp_tcp_socket_new(conn);

        if(!socket) {
            printf("Error creating TCP socket. Retrying...\n");
            amqp_destroy_connection(conn);
        } else {

            if(amqp_socket_open(socket, get_hostname(), PORT) == 0) { 

                const char *username = getenv("RABBITMQ_USER");
                const char *password = getenv("RABBITMQ_PASSWORD");

                if (!username || !password) {
                    printf("Error: RabbitMQ credentials not set in environment variables\n");
                    amqp_destroy_connection(conn);
                    exit(1);
                }

                amqp_rpc_reply_t login_reply = amqp_login(conn, "/", 0, 131072, 0, AMQP_SASL_METHOD_PLAIN, username, password);
                if(login_reply.reply_type == AMQP_RESPONSE_NORMAL) {

                    amqp_channel_open(conn, 1);

                    if(amqp_get_rpc_reply(conn).reply_type == AMQP_RESPONSE_NORMAL) {
                        printf("Successfully connected to RabbitMQ server\n");
                        return conn;
                    }
                }

                printf("Error opening channel\n");
                amqp_channel_close(conn, 1, AMQP_REPLY_SUCCESS);
            } else {
                printf("Error connecting to RabbitMQ server\n");
            }

            amqp_destroy_connection(conn);
        }

        attempt++;
        int wait_time = (attempt < max_attempts) ? attempt * 2 : 10;
        printf("Retrying connection in %d seconds...\n", wait_time);
        sleep(wait_time);
    }
}

int create_socket() {
    int attempt = 0;
    const int max_attempts = 5;
    int sock;

    while(1) {
        sock = socket(AF_INET, SOCK_STREAM, 0);
        if (sock >= 0) {
            printf("Socket created successfully\n");
            return sock;
        } else {
            attempt++;
            int wait_time = (attempt < max_attempts) ? attempt * 2 : 10;
            printf("Error creating socket. Retrying in %d seconds...\n", wait_time);
            sleep(wait_time);
        }
    }
}

int connect_to_spaceship_socket_server() {
    struct sockaddr_in server_addr; 
    int attempt = 0;
    const int max_attempts = 5;

    const char* socket_server_host = get_socket_server_hostname();
    int socket_server_port = get_socket_server_port();

    struct hostent *ip = gethostbyname(socket_server_host);
    if (ip == NULL) {
        printf("Error: Could not resolve hostname %s\n", socket_server_host);
        exit(1);
    }

    memset(&server_addr, 0, sizeof(server_addr));

    server_addr.sin_family = AF_INET;
    server_addr.sin_port = htons(socket_server_port);
    memcpy(&server_addr.sin_addr, ip->h_addr, ip->h_length);

    int sock = create_socket();

    while(1) {
        if(connect(sock, (struct sockaddr *)&server_addr, sizeof(server_addr)) == 0) {
            printf("Connected to spaceship interface via socket\n");
            return sock;
        } else {
            attempt++;
            int wait_time = (attempt < max_attempts) ? attempt * 2 : 10;
            printf("Error connecting to spaceship socket server. Retrying in %d seconds...\n", wait_time);
            sleep(wait_time);
        }
    }
}

void publish_acceleration(amqp_connection_state_t *conn, const char *json_message) {
    
    if(*conn == NULL) {
        *conn = connect_rabbitmq();
    }

    amqp_queue_declare(*conn, 1, amqp_cstring_bytes(QUEUE_NAME), 0, 1, 0, 0, amqp_empty_table);
    amqp_bytes_t queue = amqp_cstring_bytes(QUEUE_NAME);

    amqp_basic_properties_t props;
    props._flags = AMQP_BASIC_CONTENT_TYPE_FLAG;
    props.content_type = amqp_cstring_bytes("application/json");
    props.delivery_mode = 2;

    int result = amqp_basic_publish(*conn, 1, amqp_empty_bytes, queue, 0, 0, &props, amqp_cstring_bytes(json_message));

    if(result != 0 || amqp_get_rpc_reply(*conn).reply_type != AMQP_RESPONSE_NORMAL) {
        printf("Error publishing message, attempting to reconnect...\n");
        amqp_channel_close(*conn, 1, AMQP_REPLY_SUCCESS);
        amqp_connection_close(*conn, AMQP_REPLY_SUCCESS);
        amqp_destroy_connection(*conn);
        *conn = connect_rabbitmq(); 
        publish_acceleration(conn, json_message);
    }
}

int send_to_spaceship_socket_server(int sock, const char *json_message) {
    if (send(sock, json_message, strlen(json_message), 0) <= 0) {
        return -1;
    }
    else {
        return 0; 
    }
}

void read_and_publish_acceleration(const char *file_path) {
    FILE *file;
    int attempt = 0;
    const int max_attempts = 5;

    while(1) {
        file = fopen(file_path, "r");
        if(file) {
            printf("Successfully opened file\n");
            break;
        } else {
            attempt++;
            int wait_time = (attempt < max_attempts) ? attempt * 2 : 10;
            printf("Error opening file. Retrying in %d seconds...\n", wait_time);
            sleep(wait_time);
        }
    }

    char line[256];
    amqp_connection_state_t conn = connect_rabbitmq();
    int socket_conn = connect_to_spaceship_socket_server();

    while(1) {
        while(fgets(line, sizeof(line), file)) {
            line[strcspn(line, "\n")] = 0;

            Data acceleration = { atof(line) };

            char json_message[128];
            sprintf(json_message, "{\"acceleration\": %.3f}", acceleration.acceleration);

            printf("Sending acceleration value: %s\n", line);
            
            publish_acceleration(&conn, json_message);
            if(send_to_spaceship_socket_server(socket_conn, json_message) == -1) {
                close(socket_conn);
                socket_conn = connect_to_spaceship_socket_server();
            }
            sleep(3); 
        }

        rewind(file);
    }

    fclose(file);
    amqp_channel_close(conn, 1, AMQP_REPLY_SUCCESS);
    amqp_connection_close(conn, AMQP_REPLY_SUCCESS);
    amqp_destroy_connection(conn);
    close(socket_conn);
}

int main() {
    setbuf(stdout, NULL); 
    read_and_publish_acceleration(FILE_PATH);
    return 0;
}
