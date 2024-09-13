import socket
import sys

def start_server(port):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server_socket:
        server_socket.bind(('localhost', port))
        server_socket.listen()
        print(f"Servidor escutando em localhost:{port}")
        
        while True:
            conn, addr = server_socket.accept()
            with conn:
                print(f"Conectado por {addr}")
                data = conn.recv(1024).decode('utf-8')
                if not data:
                    break
                print(f"Recebido: {data}")
                response = f"World! [{port}]"
                conn.sendall(response.encode('utf-8'))

if __name__ == "__main__":
    start_server(port=int(sys.argv[1]))
