import socket

def define_ideal_weight(height, gender):

    # Calculando peso ideal
    if gender == "M":
        return (72.7 * height) - 58
    elif gender == "F":
        return (62.1 * height) - 44.7
    else:
        return -1

def start_receiver():
    try:
        receiver_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        receiver_socket.bind(('127.0.0.1', 4000))
        receiver_socket.listen()
        print('Receiver started on port 4000')

        # Espera o sender se conectar
        while True:

            # Cria um sockect específico para a comunicação entre o sender e receiver
            conn, _ = receiver_socket.accept()

            with conn:
                try:
                    sender_message = conn.recv(1024).decode().strip()
                    print(f'Received message from sender: {sender_message}')

                    # Separando os dados da mensagem, espera receber nesse formato altura:sexo
                    height, gender = sender_message.split(':')
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
                        print(f"Responded with: '{response}'")

                except ValueError:
                    conn.sendall("Invalid data format. Make sure you provide height and gender in the correct format.".encode())
                except Exception as e:
                        conn.sendall(f"Server error: {e}".encode)
    except KeyboardInterrupt:
        print("Shutting down receiver...")
    except Exception as e:
        print(f"An error occurred: {e}")
    finally:
        receiver_socket.close()
        print("Receiver stopped")

if __name__ == "__main__":
    start_receiver()