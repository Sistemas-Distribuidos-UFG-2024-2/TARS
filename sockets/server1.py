import socket

# Conexão entre o balanceador e o servidor
def start_server(port):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server_socket:
        server_socket.bind(('127.0.0.1', port))
        server_socket.listen()
        print(f'Server is running on port: {port}')

        while True:

            conn, _ = server_socket.accept()
            
            with conn:

                # Lê a mensagem do cliente transmitida pelo balanceador
                message = conn.recv(1024).decode().strip()
                print(f"Received message: '{message}' on port {port}")
                
                if message == "Hello":
                    conn.sendall("world".encode())
                    print(f"Responded with 'world' on port {port}")
                elif message == "PING":
                    conn.sendall("PONG".encode())
                    print(f"Responded with 'PONG' on port {port}")
                else:
                    conn.sendall("Unknown message".encode())
                    print(f"Responded with 'Unknown message' on port {port}")

if __name__ == "__main__":
    start_server(5001)  # Porta do servidor 1
                