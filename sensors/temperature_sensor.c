#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <amqp.h>
#include <amqp_tcp_socket.h>
#include <unistd.h>

// Endereço e porta padrão do servidor RabbitMQ
#define HOSTNAME "localhost"
#define PORT 5672
// Nome da fila em que o sensor publicará as mensagens
#define QUEUE_NAME "temperature_queue"
#define FILE_PATH "temperatures.txt"

// Função para estabelecer uma conexão com o RabbitMQ através de um socket TCP
amqp_connection_state_t connect_rabbitmq() {
    amqp_connection_state_t conn = amqp_new_connection();
    amqp_socket_t *socket = amqp_tcp_socket_new(conn);
    if(!socket) {
        fprintf(stderr, "Error creating TCP socket\n");
        exit(1);
    }

    // Tenta abrir o socket TCP
    if(amqp_socket_open(socket, HOSTNAME, PORT)) {
        fprintf(stderr, "Error connecting to RabbitMQ server\n");
        exit(1);
    }

    // Configuração de login no RabbitMQ com acesso padrão
    // TODO: Tem como tirar isso e fazer de outra forma?
    amqp_login(conn, "/", 0, 131072, 0, AMQP_SASL_METHOD_PLAIN, "guest", "guest");

    // Abre o canal 1 para comunicação
    amqp_channel_open(conn, 1);
    if(amqp_get_rpc_reply(conn).reply_type != AMQP_RESPONSE_NORMAL) {
        fprintf(stderr, "Error opening channel\n");
        exit(1);
    }

    return conn;
}

// Função para enviar a mensagem com a temperatura para a fila do RabbitMQ
void publish_temperature(amqp_connection_state_t conn, const char *message) {
    
    // Declara a fila antes de publicar para garantir que ela existe
    amqp_queue_declare(conn, 1, amqp_cstring_bytes(QUEUE_NAME), 0, 0, 0, 1, amqp_empty_table);
    amqp_bytes_t queue = amqp_cstring_bytes(QUEUE_NAME);

    // Propriedades da mensagem
    amqp_basic_properties_t props;
    props._flags = AMQP_BASIC_CONTENT_TYPE_FLAG;
    props.content_type = amqp_cstring_bytes("text/plain");

    // Publica a mensagem na fila
    amqp_basic_publish(conn, 1, amqp_empty_bytes, queue, 0, 0, &props, amqp_cstring_bytes(message));
}

// Função que lê o arquivo e publica os dados no RabbitMQ
void read_and_publish_temperature(const char *file_path) {
    FILE *file = fopen(file_path, "r");
    if(!file) {
        fprintf(stderr, "Error opening file\n");
        exit(1);
    }

    // Armazena até 255 caracteres de uma linha
    char line[256];
    amqp_connection_state_t conn = connect_rabbitmq();

    // Loop infinito para ler e publicar continuamente
    while(1) {
        // Lê uma linha do arquivo por vez, armazena em 'line' e publica como mensagem
        while(fgets(line, sizeof(line), file)) {
            // Remove caractere de nova linha da linha lida e coloca o '\0' no lugar
            line[strcspn(line, "\n")] = 0;
            printf("Sending temperature: %s\n", line);
            publish_temperature(conn, line);

            // Publica uma temperatura nova a cada 2s
            sleep(2);
        }

        // Se chegar ao fim do arquivo, volta ao início dele
        rewind(file);
    }

    // TODO: Necessário mesmo lendo todo o arquivo?
    fclose(file);
    amqp_channel_close(conn, 1, AMQP_REPLY_SUCCESS);
    amqp_connection_close(conn, AMQP_REPLY_SUCCESS);
    amqp_destroy_connection(conn);
}

int main() {
    read_and_publish_temperature(FILE_PATH);
    return 0;
}