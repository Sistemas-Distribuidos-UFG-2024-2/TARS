import Pyro5.api

@Pyro5.api.expose
class IdealWeightCalculator(object):
    def define_ideal_weight(self, height, gender):
        if gender == "M":
            return (72.7 * height) - 58
        elif gender == "F":
            return (62.1 * height) - 44.7
        else:
            return "Invalid gender. Please provide M or F."
        
def main():

    # Inicia o daemon Pyro e registra o objeto no servidor de nomes
    daemon = Pyro5.server.Daemon()
    ns = Pyro5.api.locate_ns()
    uri = daemon.register(IdealWeightCalculator)
    ns.register("define_ideal_weight", uri)
    
    print("Service is running")
    daemon.requestLoop()  # Mantém o servidor rodando

if __name__ == "__main__":
    main()