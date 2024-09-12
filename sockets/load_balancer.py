import socket 
import random
import threading
import time

# Lista (tupla) de servidores do conjunto S
servers = [
    ('127.0.0.1', 5001), # Servidor 1
    ('127.0.0.1', 5002), # Servidor 2
    ('127.0.0.1', 5003), # Servidor 3
]

# Dicionário (chave-valor) para armazenar o estado de disponibilidade dos servidores
# Inicialmente todos os servidores são marcados com falso
server_status = {server: False for server in servers}

# Intervalo de verificação de status dos servidores (em segundos)
CHECK_INTERVAL = 10

# Função que verifica periodicamente o estado dos servidores
def check_server_status():
    while True:
        for server in servers:
            try:
                # Tenta se conectar ao servidor com um timeout de 5s
                with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
                    s.settimeout(5)
                    s.connect(server)
                    s.sendall("PING".encode())
                    response = s.recv(1024).decode()

                    if response == "PONG":
                        server_status[server] = True
                        print(f"Server {server} is now available")
                    else:
                        server_status[server] = False
                        print(f"Server {server} is now unavailable")

            except (socket.timeout, socket.error):
                server_status[server] = False
                print(f"Server {server} is now unavailable")
        
        # Pausa após verificar todos os servidores antes de repetir o processo
        time.sleep(CHECK_INTERVAL)

# Encaminha a requisição do cliente para um servidor específico que esteja disponível
# server_address = (endereço IP, porta)
def forward_request_to_server(server_address, client_message):
    try:
        # Cria um socket TCP usando IPv4 e garante que este será fechado após a operação, mesmo em caso de erro
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.settimeout(5)
            s.connect(server_address)
            print(f"Forwarding message to server: '{client_message}'")
            s.sendall(client_message.encode())
            response = s.recv(1024)
            return response.decode()
    except (socket.timeout, socket.error) as e:
        print(f"Failed to connect to server {server_address}: {e}")
        return None

# Inicia o balanceador de carga
def start_load_balancer():

    # Inicia uma nova thread que executa a função check_server_status em segundo plano
    # Será encerrada assim que o programa principal encerrar
    threading.Thread(target=check_server_status, daemon=True).start()

    # Socket do balanceador de carga
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as balancer_socket:
        balancer_socket.bind(('127.0.0.1', 4000)) # Onde vai escutar as requisições
        balancer_socket.listen()
        print("Load Balancer started on port 4000")

        # Loop infinito para aceitar e processar conexões
        while True:

            # Quando um cliente tentar se conectar ao balanceador, a conexão será aceita 
            # E retorna um socket específico para a comunicação entre os dois 
            # (também retorna o endereço IP e porta do cliente que se conectou)
            client_socket, _ = balancer_socket.accept()

            with client_socket:

                # Lê a mensagem do cliente
                client_message = client_socket.recv(1024).decode()
                print(f"Received message from client: {client_message}")

                # Verifica quais servidores estão disponíveis para processar a requisição do cliente
                avaiable_servers = []
                for server in servers:
                    if server_status[server]:
                        avaiable_servers.append(server)

                if len(avaiable_servers) == 0: # Todos os servidores estão indisponíveis
                    client_socket.sendall("No avaiable servers now. Try again later.".encode())
                    print("All server are unavailable.")
                    continue # Espera por uma nova conexão de cliente
                
                # Seleciona um servidor de forma aleatória para encaminhar a mensagem do cliente
                server = random.choice(avaiable_servers)
                print(f"Forwarding message to server at {server}")
            
                # Encaminha a requisição para o servidor escolhido e aguarda uma resposta
                response = forward_request_to_server(server, client_message)

                # Se houver uma resposta válida (diferente de none), envia-a ao cliente, caso contrário, envia uma mensagem de erro
                if response: 
                    client_socket.sendall(response.encode())
                    print(f"Forwarded response to client: {response}")
                else: 
                    client_socket.sendall("Failed to connect to the server.".encode())
                    print(f"Failed to get a response from the server {server}")
    
if __name__ == "__main__":
    start_load_balancer()