import socket

def credit_calculator(average_balance):

    # Valor do crédito com base no saldo médio do cliente
    if(average_balance <= 200):
        return 0
    elif(201 <= average_balance <= 400):
        return average_balance * 0.2
    elif(401 <= average_balance <= 600):
        return average_balance * 0.3
    else: # De 601 em diante
        return average_balance * 0.4

def start_receiver():
    try:
        receiver_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        receiver_socket.bind(('127.0.0.1', 4000))
        receiver_socket.listen()
        print('Receiver started on port 4000')

        while True:

            conn, _ = receiver_socket.accept()

            with conn:
                try:
                    sender_message = conn.recv(1024).decode().strip()
                    print(f'Received message from sender: {sender_message}')

                    average_balance = float(sender_message)

                    credit = credit_calculator(average_balance)
                    response = f"Credit is {credit:.2f}"
                    conn.sendall(response.encode())
                    print(f"Responsed with: '{response}'")

                except ValueError:
                    conn.sendall("Invalid data format.".encode())
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