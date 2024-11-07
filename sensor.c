#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <amqp.h>
#include <amqp_tcp_socket.h>
#include <unistd.h>

#define HOSTNAME "host.docker.internal"
#define PORT 5672
#define QUEUE_NAME "spacecraft_internal_pressure_queue"
#define FILE_PATH "spacecraft_internal_pressure_value.txt"
#define MAX_RETRIES 5



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
            if(amqp_socket_open(socket, HOSTNAME, PORT) == 0) {

                const char *username = getenv("RABBITMQ_USER");
                const char *password = getenv("RABBITMQ_PASSWORD");
                printf("RabbitMQ credentials - User: %s, Password: %s\n", username, password);

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

void publish_radiation_value(amqp_connection_state_t *conn, const char *message) {
    
    if(*conn == NULL) {
        *conn = connect_rabbitmq();
    }

    amqp_queue_declare(*conn, 1, amqp_cstring_bytes(QUEUE_NAME), 0, 0, 0, 1, amqp_empty_table);
    amqp_bytes_t queue = amqp_cstring_bytes(QUEUE_NAME);

    amqp_basic_properties_t props;
    props._flags = AMQP_BASIC_CONTENT_TYPE_FLAG;
    props.content_type = amqp_cstring_bytes("text/plain");

    int result = amqp_basic_publish(*conn, 1, amqp_empty_bytes, queue, 0, 0, &props, amqp_cstring_bytes(message));

    if(result != 0 || amqp_get_rpc_reply(*conn).reply_type != AMQP_RESPONSE_NORMAL) {
        printf("Error publishing message, attempting to reconnect...\n");
        amqp_channel_close(*conn, 1, AMQP_REPLY_SUCCESS);
        amqp_connection_close(*conn, AMQP_REPLY_SUCCESS);
        amqp_destroy_connection(*conn);
        *conn = connect_rabbitmq();
        publish_radiation_value(conn, message);
    }
}

void read_and_publish_radiation_value(const char *file_path) {
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

    while(1) {
        while(fgets(line, sizeof(line), file)) {
            line[strcspn(line, "\n")] = 0;
            printf("Sending radiation value: %s\n", line);
            publish_radiation_value(&conn, line);
            sleep(2);
        }

        rewind(file);
    }

    fclose(file);
    amqp_channel_close(conn, 1, AMQP_REPLY_SUCCESS);
    amqp_connection_close(conn, AMQP_REPLY_SUCCESS);
    amqp_destroy_connection(conn);
}

int main() {
    setbuf(stdout, NULL);
    read_and_publish_radiation_value(FILE_PATH);
    return 0;
}
