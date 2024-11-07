from xmlrpc.server import SimpleXMLRPCServer

def credit_calculator(average_balance):

    # Valor do crédito com base no saldo médio do cliente
    if(average_balance <= 200):
        return 0.0
    elif(201 <= average_balance <= 400):
        return average_balance * 0.2
    elif(401 <= average_balance <= 600):
        return average_balance * 0.3
    else: # De 601 em diante
        return average_balance * 0.4

# Configurar o servidor RPC
def main():

    server = SimpleXMLRPCServer(("localhost", 5000))
    print("XML-RPC Server started on port 5000")
    server.register_function(credit_calculator, "credit_calculator")
    server.serve_forever()

if __name__ == "__main__":
    main()