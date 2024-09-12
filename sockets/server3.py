import socket

def start_server(port):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server_socket:
        server_socket.bind(('127.0.0.1', port))
        server_socket.listen()
        print(f'Server is running on port: {port}')

        while True:

            client_socket, _ = server_socket.accept()
            
            with client_socket:
                message = client_socket.recv(1024).decode().strip()
                print(f"Received message: '{message}' on port {port}")  # Mostra a mensagem recebida
                
                if message == "Hello":
                    client_socket.sendall("word".encode())
                    print(f"Responded with 'world' on port {port}")
                elif message == "PING":
                    client_socket.sendall("PONG".encode())
                    print(f"Responded with 'PONG' on port {port}")
                else:
                    client_socket.sendall("Unknown message".encode())
                    print(f"Responded with 'Unknown message' on port {port}")

if __name__ == "__main__":
    start_server(5003)  # Porta do servidor 3
                