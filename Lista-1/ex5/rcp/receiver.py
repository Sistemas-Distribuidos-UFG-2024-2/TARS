from xmlrpc.server import SimpleXMLRPCServer

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

# Configurar o servidor RPC
def main():

    server = SimpleXMLRPCServer(("localhost", 5000))
    print("XML-RPC Server started on port 5000")
    server.register_function(define_category, "define_category")
    server.serve_forever()

if __name__ == "__main__":
    main()