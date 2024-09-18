import socket 
import threading
import time
import random

# Lista (tupla) de servidores
servers = [
    ('127.0.0.1', 5001), # Servidor 1
    ('127.0.0.1', 5002), # Servidor 2
    ('127.0.0.1', 5003), # Servidor 3
]

# Dicionário (chave-valor) para armazenar o estado de disponibilidade dos servidores (UP = true, DOWN = false)
# Inicialmente todos os servidores são marcados com falso
server_status = {server: False for server in servers}

# Intervalo de verificação de saúde dos servidores
CHECK_INTERVAL = 5

# Função que verifica periodicamente o estado dos servidores
# Health Check -> Servidores
def check_server_health():
    # Verifica continuamento o estado dos servidores em um loop infinito
    while True:
        for server in servers:
            try:
                # Tenta se conectar ao servidor com um timeout de 3s de espera pela resposta
                with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
                    s.settimeout(3)
                    s.connect(server)
                    s.sendall("CHECK".encode())
                    response = s.recv(1024).decode()

                    if response == "OK":
                        server_status[server] = True
                        print(f"Server {server} is UP")
                    else: 
                        server_status[server] = False
                        print(f"Server {server} is DOWN")

            except (socket.timeout, socket.error):
                server_status[server] = False
                print(f"Server {server} is DOWN")
        
        # Pausa após verificar todos os servidores antes de repetir o processo
        time.sleep(CHECK_INTERVAL)

# Função que atende (responde) o balanceador quando solicitado e fornece servidores ativos
# Health Check -> Balanceador
def answer_balancer():

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as hc_socket:
        hc_socket.bind(('127.0.0.1', 7000))
        hc_socket.listen()
        print("Health Check service running on port 7000")
        
        while True:
            
            # Espera o balanceador se conectar com ele
            conn, _ = hc_socket.accept()
            
            with conn:
                
                available_servers = []
                for server in servers:
                    if server_status[server]:
                        available_servers.append(server)
                    
            
                if available_servers:
                    print(f"Available servers: {available_servers}")
                    # Se houver servidores disponíveis, escolhe um aleatório para responder
                    selected_server = random.choice(available_servers)
                    print(f"Selected server: {selected_server}")
                    conn.sendall(f"{selected_server[0]}:{selected_server[1]}".encode())
                    print(f"Sent active server {selected_server} to load balancer")  
                else:
                    conn.sendall("No available servers".encode())
                    print("No available servers")

if __name__ == "__main__":

    # Inicia a thread para verificar a saúde dos servidores
    threading.Thread(target=check_server_health).start()

    # Inicia serviço para atender o balanceador
    answer_balancer()