import Pyro5.api 

@Pyro5.api.expose
class Bank():
    def credit_calculator(self, average_balance):
        # Valor do crédito com base no saldo médio do cliente
        if(average_balance <= 200):
            return 0.0
        elif(201 <= average_balance <= 400):
            return average_balance * 0.2
        elif(401 <= average_balance <= 600):
            return average_balance * 0.3
        else: # De 601 em diante
            return average_balance * 0.4

def main():

    daemon = Pyro5.server.Daemon()
    ns = Pyro5.api.locate_ns()
    uri = daemon.register(Bank)
    ns.register("credit_calculator", uri) 
    print("Service is running")
    daemon.requestLoop()

if __name__ == "__main__":
    main()