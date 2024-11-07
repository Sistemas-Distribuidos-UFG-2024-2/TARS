#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <amqp.h>
#include <amqp_tcp_socket.h>
#include <unistd.h>

// Endereço e porta padrão do servidor RabbitMQ
#define HOSTNAME "host.docker.internal"
#define PORT 5672
// Nome da fila em que o sensor publicará as mensagens
#define QUEUE_NAME "acceleration_queue"
#define FILE_PATH "acceleration_values.txt"


/**
 * Intervalo normal de acceleração de uma espaçonave que está orbitando a Terra em velocidade constante:
 * -1 a 1 micrômetros por segundo ao quadrado
 */


// Função para estabelecer uma conexão com o RabbitMQ através de um socket TCP
amqp_connection_state_t connect_rabbitmq() {
    amqp_connection_state_t conn;
    int attempt = 0; // Se a conexão for perdida, o sensor vai tentar se reconectar automaticamente
    const int max_attempts = 5; // Número max de tentativas antes de esperar 10s (fixo)

    // Loop para estabelecer conexão com sucesso
    while(1) {
        conn = amqp_new_connection();
        amqp_socket_t *socket = amqp_tcp_socket_new(conn);

        if(!socket) {
            printf("Error creating TCP socket. Retrying...\n");
            amqp_destroy_connection(conn);
        } else {

            // Socket criado, tenta abrir o socket TCP
            if(amqp_socket_open(socket, HOSTNAME, PORT) == 0) { // Conexão feita com sucesso

                // Obter as credenciais do RabbitMQ das variáveis de ambiente do Docker
                const char *username = getenv("RABBITMQ_USER");
                const char *password = getenv("RABBITMQ_PASSWORD");

                if (!username || !password) {
                    printf("Error: RabbitMQ credentials not set in environment variables\n");
                    amqp_destroy_connection(conn);
                    exit(1); // Sem credenciais não tem como continuar
                }

                // Configuração de login no RabbitMQ usando as variáveis de ambiente
                amqp_rpc_reply_t login_reply = amqp_login(conn, "/", 0, 131072, 0, AMQP_SASL_METHOD_PLAIN, username, password);
                if(login_reply.reply_type == AMQP_RESPONSE_NORMAL) {

                    // Se deu tudo certo no login, tenta abrir o canal 1 para comunicação para publicar mensagens
                    amqp_channel_open(conn, 1);

                    // Verifica se o canal foi aberto com sucesso
                    if(amqp_get_rpc_reply(conn).reply_type == AMQP_RESPONSE_NORMAL) {
                        printf("Successfully connected to RabbitMQ server\n");
                        return conn;
                    }
                }

                // Se o login ou canal falharem
                printf("Error opening channel\n");
                amqp_channel_close(conn, 1, AMQP_REPLY_SUCCESS);
            } else {
                printf("Error connecting to RabbitMQ server\n");
            }

            // Falha na conexão, detrói e tenta novamente
            amqp_destroy_connection(conn);
        }

        // Aguardar antes de tentar novamente
        attempt++;
        int wait_time = (attempt < max_attempts) ? attempt * 2 : 10; // Ajuste do tempo de espera para não sobrecarregar o servidor
        printf("Retrying connection in %d seconds...\n", wait_time);
        sleep(wait_time);
    }
}

// Função para enviar a mensagem com a aceleração para a fila do RabbitMQ
void publish_acceleration(amqp_connection_state_t *conn, const char *message) {
    
    // Verifica se a conexão ainda está ativa antes de cada pub, se não estiver tenta reconectar
    // Se a conexão for interrompida, ele voltará de onde parou
    if(*conn == NULL) {
        *conn = connect_rabbitmq();
    }

    // Declara a fila antes de publicar para garantir que ela existe
    amqp_queue_declare(*conn, 1, amqp_cstring_bytes(QUEUE_NAME), 0, 0, 0, 1, amqp_empty_table);
    amqp_bytes_t queue = amqp_cstring_bytes(QUEUE_NAME);

    // Propriedades da mensagem
    amqp_basic_properties_t props;
    props._flags = AMQP_BASIC_CONTENT_TYPE_FLAG;
    props.content_type = amqp_cstring_bytes("text/plain");

    // Tenta publicar a mensagem na fila
    int result = amqp_basic_publish(*conn, 1, amqp_empty_bytes, queue, 0, 0, &props, amqp_cstring_bytes(message));

    // Verifica se a publicação foi bem-sucedida, caso contrário fecha e destrói a conexão, tenta reconectar e publicar novamente
    if(result != 0 || amqp_get_rpc_reply(*conn).reply_type != AMQP_RESPONSE_NORMAL) {
        printf("Error publishing message, attempting to reconnect...\n");
        amqp_channel_close(*conn, 1, AMQP_REPLY_SUCCESS);
        amqp_connection_close(*conn, AMQP_REPLY_SUCCESS);
        amqp_destroy_connection(*conn);
        *conn = connect_rabbitmq(); // Tenta conectar de novo
        publish_acceleration(conn, message);
    }
}

// Função que lê o arquivo e publica os dados no RabbitMQ
void read_and_publish_acceleration(const char *file_path) {
    FILE *file;
    int attempt = 0;
    const int max_attempts = 5;

    // Loop para tentar abrir o arquivo até que seja bem-sucedido
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

    // Armazena até 255 caracteres de uma linha
    char line[256];
    amqp_connection_state_t conn = connect_rabbitmq();

    // Loop infinito para ler e publicar continuamente
    while(1) {
        // Lê uma linha do arquivo por vez, armazena em 'line' e publica como mensagem
        while(fgets(line, sizeof(line), file)) {
            // Remove caractere de nova linha da linha lida e coloca o '\0' no lugar
            line[strcspn(line, "\n")] = 0;
            printf("Sending acceleration: %s\n", line);
            publish_acceleration(&conn, line);
            sleep(2); // Publica uma aceleração nova a cada 2s
        }

        // Se chegar ao fim do arquivo, volta ao início dele
        rewind(file);
    }

    // É só para prevenir caso um imprevisto acontença, como o ser loop interrompido manualmente
    fclose(file);
    amqp_channel_close(conn, 1, AMQP_REPLY_SUCCESS);
    amqp_connection_close(conn, AMQP_REPLY_SUCCESS);
    amqp_destroy_connection(conn);
}

int main() {
    setbuf(stdout, NULL); // Imprimir imediatamente
    read_and_publish_acceleration(FILE_PATH);
    return 0;
}