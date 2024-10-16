import socket

def define_category(age):

    # Classificação do nadador com base na idade
    if(5 <= age <= 7):
        return "Infantil A"
    elif(8 <= age <= 10):
        return "Infantil B"
    elif(11 <= age <= 13):
        return "Juvenil A"
    elif(14 <= age <= 17):
        return "Juvenil B"
    else:
        return "Adulto"

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

                    age = int(sender_message)

                    response = define_category(age)
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