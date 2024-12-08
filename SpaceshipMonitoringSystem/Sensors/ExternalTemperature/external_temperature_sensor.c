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
// Nome da fila em que o sensor publicará as mensagens
#define QUEUE_NAME "external_temperature_queue"
#define FILE_PATH "external_temperatures.txt"

typedef struct {
    float temp;
} Data;


/**
 * Intervalo normal de temperatura de uma espaçonave que está próxima à Terra (órbita baixa terrestre): -150ºC a +150ºC
 */

// Função que obtém o hostname a partir de variável de ambiente
const char* get_hostname() {
    const char* hostname = getenv("RABBITMQ_HOSTNAME");
    if (hostname == NULL) {
        printf("Error: HOSTNAME environment variable not set\n");
        exit(1);
    }

    return hostname;
}

// Função para obter o hostname do servidor de sockets
const char* get_socket_server_hostname() {
    const char* hostname = getenv("SOCKET_SERVER_HOSTNAME");
    if (hostname == NULL) {
        printf("Error: SOCKET_SERVER_HOSTNAME environment variable not set\n");
        exit(1);
    }

    return hostname;
}

// Função para obter a porta do servidor de sockets
int get_socket_server_port() {
    const char* port_str = getenv("SOCKET_SERVER_PORT");
    if (port_str == NULL) {
        printf("Error: SOCKET_SERVER_PORT environment variable not set\n");
        exit(1);
    }
    
    return atoi(port_str);
}

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
            if(amqp_socket_open(socket, get_hostname(), PORT) == 0) { // Conexão feita com sucesso

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

// Função para criar o socket de comunicação com a nave espacial
int create_socket() {
    int attempt = 0;
    const int max_attempts = 5;
    int sock;

    // Loop para garantir que o socket seja criado
    while(1) {
        sock = socket(AF_INET, SOCK_STREAM, 0);
        if (sock >= 0) {
            printf("Socket created successfully\n");
            // Retorna o socket criado
            return sock;
        } else {
            attempt++;
            int wait_time = (attempt < max_attempts) ? attempt * 2 : 10;
            printf("Error creating socket. Retrying in %d seconds...\n", wait_time);
            sleep(wait_time);
        }
    }
}

// Função para conectar o socket ao servidor da nave espacial
int connect_to_spaceship_socket_server() {
    struct sockaddr_in server_addr; // Estrutura para armazenar os dados do servidor
    int attempt = 0;
    const int max_attempts = 5;

    // Obtém hostname e porta do servidor de sockets via variáveis de ambiente
    const char* socket_server_host = get_socket_server_hostname();
    int socket_server_port = get_socket_server_port();

    // Resolve o hostname para um endereço IP válido
    struct hostent *ip = gethostbyname(socket_server_host);
    if (ip == NULL) {
        printf("Error: Could not resolve hostname %s\n", socket_server_host);
        exit(1);
    }

    // Inicializa a estrutura server_addr com zeros
    memset(&server_addr, 0, sizeof(server_addr));

    // Preenche os dados do servidor
    server_addr.sin_family = AF_INET; // IPv4
    server_addr.sin_port = htons(socket_server_port); // Porta
    memcpy(&server_addr.sin_addr, ip->h_addr, ip->h_length); // Copia o endereço IP resolvido para o campo sin_addr, o length limita quantos dados serão copiados

    int sock = create_socket();

    // Loop para tentativa de conexão com o servidor
    while(1) {
        if(connect(sock, (struct sockaddr *)&server_addr, sizeof(server_addr)) == 0) {
            printf("Connected to spaceship interface via socket\n");
            // Retorna o socket conectado com o servidor
            return sock;
        } else {
            attempt++;
            int wait_time = (attempt < max_attempts) ? attempt * 2 : 10;
            printf("Error connecting to spaceship socket server. Retrying in %d seconds...\n", wait_time);
            sleep(wait_time);
        }
    }
}

// Função para enviar a mensagem com a temperatura para a fila do RabbitMQ
void publish_temperature(amqp_connection_state_t *conn, const char *json_message) {
    
    // Verifica se a conexão ainda está ativa antes de cada pub, se não estiver tenta reconectar
    // Se a conexão for interrompida, ele voltará de onde parou
    if(*conn == NULL) {
        *conn = connect_rabbitmq();
    }

    // Declara a fila antes de publicar para garantir que ela existe
    amqp_queue_declare(*conn, 1, amqp_cstring_bytes(QUEUE_NAME), 0, 1, 0, 0, amqp_empty_table);
    amqp_bytes_t queue = amqp_cstring_bytes(QUEUE_NAME);

    // Propriedades da mensagem
    amqp_basic_properties_t props;
    props._flags = AMQP_BASIC_CONTENT_TYPE_FLAG;
    props.content_type = amqp_cstring_bytes("application/json");
    props.delivery_mode = 2;

    // Tenta publicar a mensagem na fila
    int result = amqp_basic_publish(*conn, 1, amqp_empty_bytes, queue, 0, 0, &props, amqp_cstring_bytes(json_message));

    // Verifica se a publicação foi bem-sucedida, caso contrário fecha e destrói a conexão, tenta reconectar e publicar novamente
    if(result != 0 || amqp_get_rpc_reply(*conn).reply_type != AMQP_RESPONSE_NORMAL) {
        printf("Error publishing message, attempting to reconnect...\n");
        amqp_channel_close(*conn, 1, AMQP_REPLY_SUCCESS);
        amqp_connection_close(*conn, AMQP_REPLY_SUCCESS);
        amqp_destroy_connection(*conn);
        *conn = connect_rabbitmq(); // Tenta conectar de novo
        publish_temperature(conn, json_message);
    }
}

// Envia a mensagem JSON ao servidor da nave espacial através do socket
int send_to_spaceship_socket_server(int sock, const char *json_message) {
    
    if (send(sock, json_message, strlen(json_message), 0) == -1) {
        // Falha no envio: Fecha o socket e retorna erro
        close(sock);
        return -1; // Indica que a conexão precisa ser reestabelecida
    }
    return 0; // Envio bem-sucedido
}

// Função que lê o arquivo e publica os dados no RabbitMQ
void read_and_publish_temperature(const char *file_path) {
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
    int socket_conn = connect_to_spaceship_socket_server();

    // Loop infinito para ler e publicar continuamente
    while(1) {
        // Lê uma linha do arquivo por vez, armazena em 'line' e publica como mensagem
        while(fgets(line, sizeof(line), file)) {
            // Remove caractere de nova linha da linha lida e coloca o '\0' no lugar
            line[strcspn(line, "\n")] = 0;

            // Preenche struct com valor lido
            Data temperature = { atof(line) };

            // Converte valor da struct em um JSON
            char json_message[128];
            sprintf(json_message, "{\"external_temperature\": %.1f}", temperature.temp);

            printf("Sending external temperature value: %s\n", line);
            
            // Publica no RabbitMQ
            publish_temperature(&conn, json_message);
            // Envia para a nave espacial via comunicação direta
            if(send_to_spaceship_socket_server(socket_conn, json_message) == -1) {
                socket_conn = connect_to_spaceship_socket_server();
            }
            
            // Pausa por 3s antes de publicar uma nova temperatura
            sleep(3); 
        }

        // Se chegar ao fim do arquivo, volta ao início dele
        rewind(file);
    }

    // É só para prevenir caso um imprevisto acontença, como o ser loop interrompido manualmente
    fclose(file);
    amqp_channel_close(conn, 1, AMQP_REPLY_SUCCESS);
    amqp_connection_close(conn, AMQP_REPLY_SUCCESS);
    amqp_destroy_connection(conn);
    close(socket_conn);
}

int main() {
    setbuf(stdout, NULL); // Imprimir imediatamente
    read_and_publish_temperature(FILE_PATH);
    return 0;
}