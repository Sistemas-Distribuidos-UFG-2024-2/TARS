import socket
import threading
import time

def send_request_to_server(server_address, message, response_queue):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as client_socket:
        client_socket.settimeout(2)  
        try:
            client_socket.connect(server_address)
            client_socket.sendall(message.encode('utf-8'))
            response = client_socket.recv(1024).decode('utf-8')
            response_queue.append(response)
        except socket.timeout:
            print(f"Servidor {server_address} não respondeu a tempo")
        except ConnectionRefusedError:
            print(f"Conexão recusada para o servidor {server_address}")

def client_request(servers, message):
    responses = []
    threads = []

    for server in servers:
        send_request_to_server(server, message, responses)
        if responses:
            return responses[0]  
        
    return "Nenhuma resposta recebida"

"""   for server in servers:
        thread = threading.Thread(target=send_request_to_server, args=(server, message, responses))
        thread.start()
        threads.append(thread)
    
    for thread in threads:
        thread.join(timeout=5)
    
    if responses:
        return responses[0]  
    else:
        return "Nenhuma resposta recebida"
 """
if __name__ == "__main__":
    servers = [('localhost', 65432), ('localhost', 65433), ('localhost', 65434)]
    message = "Hello"
    while True:
        print(f"Enviando {message}")
        response = client_request(servers, message)
        print(f"Resposta recebida: {response}")
        time.sleep(3)