# Import para uma comunicação RMI-like
import Pyro5.api 

# Expondo a classe e seus métodos para que sejam acessíveis remotamente para os clientes
@Pyro5.api.expose
class IdealWeightCalculator():
    # self -> método de instância, pertence a uma instância da classe, e não à classe como um todo
    def define_ideal_weight(self, height, gender):
        if gender == "M":
            return (72.7 * height) - 58
        else: # F
            return (62.1 * height) - 44.7

# Configura e inicia o servidor Pyro5
# Torna o serviço disponível para que os clientes possam se conectar e utilizar o método remoto       
def main():

    # Daemon: gerencia comunicação entre cliente e servidor, escuta as requisições e invoca os métodos apropriados
    # Inicia o daemon do servidor Pyro responsável por gerenciar o serviço
    daemon = Pyro5.server.Daemon()

    # O Pyro5 possui um servidor de nomes (catálogo de serviços), permite que os clientes descubram a localização dos serviços pelo nome, sem precisar saber o endereço exato do servidor
    # Localiza o servidor de nomes do Pyro5
    ns = Pyro5.api.locate_ns()

    # Daemon registra o serviço com um nome específico no servidor de nomes
    # Registra a classe no daemon, gerando uma referência única (URI) para essa classe, o que permite o cliente identificá-la e se conectar ao serviço
    uri = daemon.register(IdealWeightCalculator)
    # Associa o nome específico com a URI do serviço no servidor de nomes
    # Permite que o cliente localize o serviço apenas com o nome
    ns.register("define_ideal_weight", uri)
    
    print("Service is running")
    daemon.requestLoop()  # Mantém o servidor (daemon) rodando/ativo aguardando requisições de clientes

if __name__ == "__main__":
    main()