#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <string.h>
#include <amqp.h>
#include <amqp_tcp_socket.h>

#define HOSTNAME "host.docker.internal"
#define PORT 5672
#define QUEUE_NAME "gyroscope_queue"
#define FILE_PATH "gyroscope_values.txt"

/* Giroscópios são extremamente importantes para navegação. 
    Eles ajudam na orientação da direção em que um corpo se move pelo espaço.
    Os giroscópios modernos oferecem 3 tipos de dados diferentes:
    Eixo X: Conhecido como Pitch e está relacionado à inclinação do corpo para cima ou para baixo (rotação em torno do eixo horizontal)
    Eixo Y: Conhecido como Roll e está relacionado à inclinação do corpo para a esquerda ou para a direita (rotação em torno do eixo lateral)
    Eixo Z: Conhecido como Yaw e está relacionado à inclinação do corpo para os lados, gerando profundidade (rotação em torno do eixo vertical)
    Os valores dos eixos são recebidos na unidade de graus por segundo e representam a velocidade angular do movimento de um corpo sobre os eixos espaciais XYZ.
    Esses valores podem ser positivos ou negativos.
*/

amqp_connection_state_t connect_rabbitmq() {
    amqp_connection_state_t connect_rabbitmq();
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

void publish_gyroscope(amqp_connection_state_t *conn, const char *message) {
    
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
        publish_gyroscope(conn, message);
    }
}

void read_and_pub_gyroscope(const char *file_path) {
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

    const unsigned MAX_LENGTH = 16;
    char buffer[MAX_LENGTH];
    amqp_connection_state_t conn = connect_rabbitmq();

    while(1) {
        // Função que lê a linha e aloca na string (vetor) "buffer"
        while (fgets(buffer, MAX_LENGTH, file)){
            buffer[strcspn(buffer, "\n")] = 0;
            switch(buffer[0]){
                case 'x':
                    printf("Sending value: %s\n", buffer);
                    publish_gyroscope(&conn, buffer);
                    sleep(2);
                    break;
                case 'y':
                    printf("Sending value: %s\n", buffer);
                    publish_gyroscope(&conn, buffer);
                    sleep(2);
                    break;
                case 'z':
                    printf("Sending value: %s\n", buffer);
                    publish_gyroscope(&conn, buffer);
                    // Como o eixo Z é o último dado esperado a cada vez que uma leitura do giroscópio é feita, 
                    // há um sleep apenas para evitar que os dados do arquivo (que simulam o sensor) sejam lidos instantaneamente
                    sleep(5); 
                    break;
                default:
                    printf("Error in data validation");  
            }
        }

        // Se chegar ao fim do arquivo, volta ao início dele
        rewind(file);
    }

    fclose(file);
    amqp_channel_close(conn, 1, AMQP_REPLY_SUCCESS);
    amqp_connection_close(conn, AMQP_REPLY_SUCCESS);
    amqp_destroy_connection(conn);
}

int main() {
    setbuf(stdout, NULL);
    read_and_pub_gyroscope(FILE_PATH);
    return 0;
}