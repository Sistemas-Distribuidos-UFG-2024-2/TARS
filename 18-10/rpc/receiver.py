# Receiver (Python) - XML-RPC Server using SimpleXMLRPCServer

from xmlrpc.server import SimpleXMLRPCServer

# Função para definir o peso ideal
def define_ideal_weight(height, gender):
    if gender == "M":
        return (72.7 * height) - 58
    elif gender == "F":
        return (62.1 * height) - 44.7
    else:
        return "Invalid gender. Please provide M or F."

# Configurar o servidor RPC
def main():
    # Iniciando o servidor na porta 5000
    server = SimpleXMLRPCServer(("localhost", 5000))
    print("Servidor XML-RPC rodando na porta 5000...")

    # Registrar a função no servidor para que fique acessível via RPC
    server.register_function(define_ideal_weight, "define_ideal_weight")

    # Manter o servidor rodando
    server.serve_forever()

if __name__ == "__main__":
    main()