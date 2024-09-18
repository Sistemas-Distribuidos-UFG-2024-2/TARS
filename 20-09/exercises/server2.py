import socket
import threading
from ideal_weight import define_ideal_weight

# Ideia: recebem as requisições dos clientes e criam uma nova thread para cada requisição, permitindo processamento simultâneo

# Lida com a requisição do cliente, encaminhada pelo balanceador
def handle_request(conn, port):
    with conn:
        try:

            client_message = conn.recv(1024).decode().strip()

            if client_message == "CHECK": # Mensagem do Health Check
                conn.sendall("OK".encode())
            else:
                print(f"Received message: '{client_message}' on port {port}")

                # Separando os dados da mensagem, espera receber nesse formato altura:sexo
                height, gender = client_message.split(':')
                height = float(height)

                # Calcula o peso ideal
                ideal_weight_value = define_ideal_weight(height, gender)

                if ideal_weight_value == -1:
                    response = f"Invalid gender. Please provide M or F."
                    conn.sendall(response.encode())
                    print("Invalid gender")
                else: 
                    response = f"Ideal weight is {ideal_weight_value:.2f} kg"
                    conn.sendall(response.encode())
                    print(f"Responded with: '{response}' on port {port}")
        except ValueError:
            conn.sendall("Invalid data format. Make sure you provide height and gender in the correct format.".encode())
        except Exception as e:
            conn.sendall(f"Server error: {e}".encode)

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
    start_server(5002)  # Porta do servidor 2