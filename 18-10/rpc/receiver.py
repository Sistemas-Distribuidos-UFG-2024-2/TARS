# Criar servidor que processa chamadas XML-RPC
from xmlrpc.server import SimpleXMLRPCServer

# Permite que o cliente execute métodos no servidor, que está em uma máquina diferente
# O servidor possui funções que podem ser chamadas remotamente
# O XML é usado para estruturar as requisições e respostas, enquanto o HTTP é usado como meio de transportes para essas mensagens 

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
    print("XML-RPC Server started on port 5000")

    # Registrar a função no servidor para que o cliente possa chamá-la por XML-RPC
    # Associa o nome usado no XML pelo cliente com a função real
    server.register_function(define_ideal_weight, "define_ideal_weight")

    # Manter o servidor rodando por tempo indeterminado
    server.serve_forever()

if __name__ == "__main__":
    main()