#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <string.h>
#include <amqp.h>
#include <amqp_tcp_socket.h>

    /*Giroscópios são extremamente importantes para navegação. 
    Eles ajudam na orientação da direção em que um corpo se move pelo espaço.
    Os giroscópios modernos oferecem 3 tipos de dados diferentes:
    Eixo X: Conhecido como Pitch e está relacionado à inclinação do corpo para cima ou para baixo (rotação em torno do eixo horizontal)
    Eixo Y: Conhecido como Roll e está relacionado à inclinação do corpo para a esquerda ou para a direita (rotação em torno do eixo lateral)
    Eixo Z: Conhecido como Yaw e está relacionado à inclinação do corpo para os lados, gerando profundidade (rotação em torno do eixo vertical)
    Os valores dos eixos são recebidos na unidade de graus por segundo e representam a velocidade angular do movimento de um corpo sobre os eixos espaciais XYZ.
    Esses valores podem ser positivos ou negativos.
    */


// Endereço e porta padrão do servidor RabbitMQ
#define HOSTNAME "host.docker.internal"
#define PORT 5672
// Nome da fila em que o sensor publicará as mensagens
#define QUEUE_NAME "gyroscope_queue"
#define FILE_PATH "gyroscopeData.txt"




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

// Função para enviar a mensagem com os dados do giroscópio para a fila do RabbitMQ
void publish_gyroscope(amqp_connection_state_t *conn, const char *message) {
    
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
        publish_gyroscope(conn, message);
    }
}

void read_and_pub_gyroscope(const char *file_path) {
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

    // Leitura de linha por linha, com tamanho máximo de 16 bytes(por linha)
    const unsigned MAX_LENGTH = 16;
    char buffer[MAX_LENGTH];
    amqp_connection_state_t conn = connect_rabbitmq();

    // Loop infinito para ler e publicar continuamente
    while(1) {
        // Lê uma linha do arquivo por vez, armazena em 'line' e publica como mensagem

        while (fgets(buffer, MAX_LENGTH, file)){ //Função que lê a linha e aloca na string(vetor) "buffer"
            buffer[strcspn(buffer, "\n")] = 0; //Remove a quebra de linha /n e adiciona o /0
            printf("Enviando eixo %c", buffer[0]);
            switch(buffer[0]){
                case 'x':
                    publish_gyroscope(&conn, buffer);
                    sleep(1);
                break;
                case 'y':
                    publish_gyroscope(&conn, buffer);
                    sleep(1);
                break;
                case 'z':
                    publish_gyroscope(&conn, buffer);
                    sleep(2); 
                    //Como o eixo Z é o último dado esperado a cada vez que uma leitura do giroscópio é feita, 
                    //há um sleep apenas para evitar que os dados do arquivo(que simulam o sensor) sejam lidos instantaneamente

                break;
                default:
                    printf("Erro na validação dos dados");  
            }
            // Se chegar ao fim do arquivo, volta ao início dele
            rewind(file);
        }


    }
    // É só para prevenir caso um imprevisto acontença, como o ser loop interrompido manualmente
    fclose(file);
    amqp_channel_close(conn, 1, AMQP_REPLY_SUCCESS);
    amqp_connection_close(conn, AMQP_REPLY_SUCCESS);
    amqp_destroy_connection(conn);
}

// Função que lê o arquivo e publica os dados no RabbitMQ


int main() {
    setbuf(stdout, NULL); // Imprimir imediatamente
    read_and_pub_gyroscope(FILE_PATH);
    return 0;
}