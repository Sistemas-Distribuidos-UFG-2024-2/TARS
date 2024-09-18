import socket
import threading

# Ideia: recebem as requisições dos clientes e criam uma nova thread para cada requisição, permitindo processamento simultâneo

# Lida com a requisição do cliente, encaminhada pelo balanceador
def handle_request(conn, port):
    with conn:
        client_message = conn.recv(1024).decode().strip()
                
        if client_message == "Hello":
            print(f"Received message: '{client_message}' on port {port}")
            conn.sendall("World".encode())
            print(f"Responded with 'World' on port {port}")
        elif client_message == "CHECK": # Mensagem do Health Check
            conn.sendall("OK".encode())
        else:
            conn.sendall("Unknown message".encode())
            print(f"Responded with 'Unknown message' on port {port}")

# Conexão entre o balanceador e o servidor, escuta múltiplas conexões
def start_server(port):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server_socket:
        server_socket.bind(('127.0.0.1', port))
        server_socket.listen()
        print(f'Server is running on port: {port}')

        while True:

            conn, _ = server_socket.accept()

            # Cria uma nova thread para cada requisição, permitindo que atenda a múltiplos clientes simultaneamente
            threading.Thread(target=handle_request, args=(conn, port)).start()

if __name__ == "__main__":
    start_server(5001)  # Porta do servidor 1
                