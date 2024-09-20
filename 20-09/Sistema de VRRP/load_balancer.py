import socket
import random
import threading
import time

# Lista de servidores com a porta para o balanceador
servers = [
    ('127.0.0.1', 5005),  # Servidor 1
    ('127.0.0.1', 5006),  # Servidor 2
    ('127.0.0.1', 5007),  # Servidor 3
]

# Dicionário para armazenar o estado de disponibilidade dos servidores
server_status = {server: False for server in servers}
CHECK_INTERVAL = 10  # Intervalo de verificação de status dos servidores (em segundos)

# Variáveis para controle do servidor primário/backup
port_primary = 5000  # Porta virtual para o servidor primário
is_primary = False
is_active = False

def handle_client(client_socket):
    message = "Servidor ativo está respondendo!\n"
    client_socket.send(message.encode("utf-8"))
    client_socket.close()

def start_server(port):
    global is_active
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind(("127.0.0.1", port))
    server_socket.listen(5)
    is_active = True
    print(f"Servidor rodando na porta {port}")

    while True:
        client_socket, addr = server_socket.accept()
        #print(f"Conexão recebida de {addr}")
        client_handler = threading.Thread(target=handle_client, args=(client_socket,))
        client_handler.start()

def send_heartbeat():
    while True:
        time.sleep(2)
        if is_primary:
            try:
                heartbeat_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
                heartbeat_socket.sendto(b"heartbeat", ("127.0.0.1", port_primary))
                heartbeat_socket.close()
            except Exception as e:
                print(f"Erro ao enviar heartbeat: {e}")

def check_server_status():
    while True:
        for server in servers:
            try:
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
        
        time.sleep(CHECK_INTERVAL)

def forward_request_to_server(server_address, client_message):
    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.settimeout(5)
            s.connect(server_address)
            print(f"Forwarding message to server: '{client_message}'")
            s.sendall(client_message.encode())
            response = s.recv(1024).decode()
            return response
    except (socket.timeout, socket.error) as e:
        print(f"Failed to connect to server {server_address}: {e}")
        return None

def start_load_balancer():
    while not is_primary:  # Espera até que o servidor assuma o papel de primário
        time.sleep(1)

    threading.Thread(target=check_server_status, daemon=True).start()

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as balancer_socket:
        balancer_socket.bind(('127.0.0.1', 4000))
        balancer_socket.listen()
        print("Load Balancer started on port 4000")

        while True:
            conn, _ = balancer_socket.accept()

            with conn:
                client_message = conn.recv(1024).decode()
                print(f"Received message from client: {client_message}")

                available_servers = [server for server in servers if server_status[server]]

                if len(available_servers) == 0:
                    conn.sendall("No available servers now. Try again later.".encode())
                    continue
                
                server = random.choice(available_servers)
                print(f"Forwarding message to server at {server}")
                response = forward_request_to_server(server, client_message)

                if response:
                    conn.sendall(response.encode())
                    print(f"Forwarded response to client: {response}")
                else:
                    conn.sendall("Failed to connect to the server.".encode())
                    print(f"Failed to get a response from the server {server}")

def check_primary_status():
    global is_primary
    while True:
        if not is_primary:
            try:
                primary_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                primary_socket.connect(("127.0.0.1", port_primary))
                primary_socket.close()  # Se a conexão for bem-sucedida, já existe um primário
                print("Servidor primário já está ativo.")
            except ConnectionRefusedError:
                print("Nenhum servidor primário ativo. Assumindo controle na porta 5000.")
                is_primary = True
                start_server(port_primary)
                break
        time.sleep(5)

def monitor_primary():
    global is_primary, is_active
    while True:
        if is_primary and not is_active:
            print("Servidor primário falhou. Backup assumindo controle.")
            is_primary = False
            start_server(port_primary)  # O backup assume
            break
        time.sleep(5)

if __name__ == "__main__":
    threading.Thread(target=check_primary_status).start()
    threading.Thread(target=monitor_primary).start()
    start_load_balancer()
