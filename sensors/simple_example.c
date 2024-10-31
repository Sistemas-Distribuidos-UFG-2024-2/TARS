#include <stdio.h>
#include <stdlib.h>
#include <amqp.h>
#include <amqp_tcp_socket.h>

#define HOSTNAME "localhost"
#define PORT 5672
#define QUEUE_NAME "test_queue"

int main() {

    // Conexão com o RabbitMQ
    amqp_connection_state_t conn = amqp_new_connection();
    amqp_socket_t *socket = amqp_tcp_socket_new(conn);
    if(!socket) {
        fprintf(stderr, "Error creating TCP socket\n");
        return 1;
    }

    if(amqp_socket_open(socket, HOSTNAME, PORT)) {
        fprintf(stderr, "Error connecting to RabbitMQ server\n");
        return 1;
    }

    // Configuração de login com o RabbitMQ
    // TODO: Tem como tirar isso e fazer de outra forma?
    amqp_login(conn, "/", 0, 131072, 0, AMQP_SASL_METHOD_PLAIN, "guest", "guest");

    // Abre o canal para comunicação
    amqp_channel_open(conn, 1);
    if(amqp_get_rpc_reply(conn).reply_type != AMQP_RESPONSE_NORMAL) {
        fprintf(stderr, "Error opening channel\n");
        return 1;
    }

    // Declara a fila de mensagens, caso não exista ainda
    amqp_queue_declare(conn, 1, amqp_cstring_bytes(QUEUE_NAME), 0, 0, 0, 1, amqp_empty_table);
    if(amqp_get_rpc_reply(conn).reply_type != AMQP_RESPONSE_NORMAL) {
        fprintf(stderr, "Error declaring queue\n");
        return 1;
    }

    // Mensagem de teste a ser enviada
    char *message = "Hello from C!";
    amqp_bytes_t message_bytes = amqp_cstring_bytes(message);

    // Publica a mensagem na fila
    amqp_basic_publish(conn, 1, amqp_empty_bytes, amqp_cstring_bytes(QUEUE_NAME), 0, 0, NULL, message_bytes);
    printf("Message sent: %s\n", message);

    // Fecha os recursos utilizados, canal e conexão
    amqp_channel_close(conn, 1, AMQP_REPLY_SUCCESS);
    amqp_connection_close(conn, AMQP_REPLY_SUCCESS);
    amqp_destroy_connection(conn);

    return 0;
}