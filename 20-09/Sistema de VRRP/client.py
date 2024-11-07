import socket

def main():
    # Abordagem: O cliente interage com um balanceador de carga que distribui as requisições entre servidores.
    
    balancer_address = "127.0.0.1"  # Endereço IP do balanceador de carga
    balancer_port = 4000  # Porta na qual o balanceador está escutando
    message = "Hello"

    try:
        # Cria uma conexão TCP entre o cliente e o balanceador de carga
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
            sock.connect((balancer_address, balancer_port))  # Conecta ao balanceador
            
            # Envia a mensagem para o balanceador
            sock.sendall(message.encode('utf-8'))  # Envia a mensagem
            
            # Lê a resposta do balanceador
            response = sock.recv(1024).decode('utf-8')  # Lê até 1024 bytes
            print(f"Response from server: {response}")

    except socket.error as e:
        # Captura qualquer erro encontrado durante o processo de conexão ou comunicação
        print(f"Socket error: {e}")
        print("Failed to connect to the load balancer.")

if __name__ == "__main__":
    main()
