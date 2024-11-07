import socket 
import threading

# Ideia: Recebe conexões dos clientes, cria uma nova thread para cada cliente e consulta o serviço de health check para encaminhar a requisição do cliente para um servidor disponível
# O balanceador pode processar várias requisições (clientes) ao mesmo tempo, sem que uma requisição bloqueie a outra


# Função responsável por se conectar com o serviço de health check para obter quais servidores estão ativos
# Balanceador -> Health Check
def get_active_servers():
    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.connect(('127.0.0.1', 7000))

            # Recebe mensagem do health check
            server_info = s.recv(1024).decode()

            if "No available servers" in server_info:
                print("No active servers now")
                return None
            else: 
                host, port = server_info.split(':')
                return (host, int(port)) # Servidor ativo
            
    except(socket.timeout, socket.error) as e:
        print(f"Failed to connect to health check: {e}")
        return None

# Função que lida com a conexão de cada cliente de forma independente (cada chamada dessa função é executada em uma thread separada)
# Balanceador -> Servidor disponível -> Balanceador -> Cliente
def handle_client(conn):
    with conn:

        # Recebe a mensagem do cliente
        client_message = conn.recv(1024).decode()
        print(f"Received message from client: {client_message}")

        # Consulta o health check para obter um servidor ativo
        available_server = get_active_servers()

        # Todos os servidores estão indisponíveis, encerra a conexão e não executa o restante do código
        if not available_server:
            conn.sendall("No available servers now. Try again later.".encode())
            print("No available servers")
            return
        
        # Redireciona a mensagem do cliente para o servidor
        try:
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
                s.settimeout(3)
                s.connect(available_server)
                print(f"Forwarding message to server at {available_server}'")
                s.sendall(client_message.encode())
                response = s.recv(1024).decode() # Recebe resposta do servidor
                conn.sendall(response.encode()) # Envia a resposta do servidor de volta ao cliente
                print(f"Sent response to client: {response}")
        except (socket.timeout, socket.error) as e:
                conn.sendall(f"Failed to connect to server.".encode())
                print(f"Failed to get a response from the server {available_server}:{e}")

# Ideia: para cada nova conexão cliente, cria uma thread separada que será responsável por processar a requisição do cliente
# Cliente -> Balanceador
def start_load_balancer():

    # Socket do balanceador de carga
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as balancer_socket:
        balancer_socket.bind(('127.0.0.1', 4000)) # Onde vai escutar as requisições
        balancer_socket.listen()
        print("Load Balancer started on port 4000")

        # Loop infinito para aceitar e processar conexões de cliente
        while True:

            # Quando um cliente tentar se conectar ao balanceador, a conexão será aceita 
            # E retorna um socket específico para a comunicação entre os dois (também retorna o endereço IP e porta do cliente que se conectou)
            conn, _ = balancer_socket.accept()

            # Cria uma nova thread para cada conexão de cliente
            threading.Thread(target=handle_client, args=(conn,)).start()
    
if __name__ == "__main__":
    start_load_balancer()