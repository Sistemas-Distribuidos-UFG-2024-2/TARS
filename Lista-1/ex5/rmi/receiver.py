import Pyro5.api 

@Pyro5.api.expose
class Swimmer():
    def define_category(self, age):
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

def main():

    daemon = Pyro5.server.Daemon()
    ns = Pyro5.api.locate_ns()
    uri = daemon.register(Swimmer)
    ns.register("define_category", uri) 
    print("Service is running")
    daemon.requestLoop()

if __name__ == "__main__":
    main()