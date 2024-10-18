# Biblioteca que facilita a criação de aplicações distribuídas e a comunicação entre objetos remotos localizados em diferentes máquinas
# Permite que um programa em Python invoque métodos em objetos que estão localizados em outro computador, como se esses objetos estivessem na mesma máquina
import Pyro5.api

# RMI permite a execução remota de métodos em objetos localizados em outra máquina
# O servidor cria métodos que podem ser chamados remotamente
# Esses métodos são expostos para que o cliente possa acessá-los remotamente
# Tem um servidor de nomes onde os serviços (objetos remotos com seus métodos) são registrados para que os clientes possam localizá-los
# O cliente se conecta com o servidor de nomes primeiro, obtém as informações para acessar o servidor, aí sim se conecta ao servidor em si
# O cliente utiliza uma URI (identificador) para se conectar ao servidor de nomes e invocar os métodos remotamente
# Nesse caso o cliente não precisa saber o IP nem porta do servidor justamente por conta do servidor de nomes, pois é ele quem possui essas informações


# Fluxo: O servidor é iniciado, registra o serviço 'IdealWeightCalculator' com seus métodos no servidor de nomes e espera por chamadas de clientes.
# O cliente se conecta ao serviço pelo nome dele.
# O cliente coleta os dados do usuário, faz as validações necessárias e faz uma chamada remota ao método 'define_ideal_weight' do serviço.
# O servidor calcula o peso ideal e retorna o resultado para o cliente. O cliente recebe a resposta e a exibe para o usuário.

def main():

    # Cria uma referência ao nome do serviço registrado no servidor de nomes (consegue acessar o serviço sem precisar saber o endereço do servidor)
    uri = "PYRONAME:define_ideal_weight"
    # Cria um proxy para o serviço remoto, o que permite que o cliente invoque métodos como se fossem métodos locais
    # Essa variável representa o serviço 'IdealWeightCalculator' do servidor, permitindo que o cliente chame o método 'define_ideal_weight'
    ideal_weight_service = Pyro5.api.Proxy(uri)

    height_input = input("Enter your height (in meters): ").strip()
    if not height_input:
        print("Height cannot be empty.")
        exit()
    
    try:
        height = float(height_input)
        if height < 1.0:
            print("Invalid height. Enter a valid positive number greater than or equal to 1 meter.")
            exit()
    except ValueError:
        print("Invalid height. Please enter a numeric value.")
        exit()
    
    gender = input("Enter your gender (F/M): ").strip().upper()
    if not gender:
        print("Gender cannot be empty.")
        exit()
    if gender not in ["M", "F"]:
        print("Invalid gender. Use M for male or F for female.")
        exit()

    # Chama o método remoto através do proxy criado anteriormente
    try:
        ideal_weight = ideal_weight_service.define_ideal_weight(height, gender)
        print(f"Ideal weight: {ideal_weight:.2f} kg")
    except Exception as e:
        print(f"Failed to connect to the Pyro5 server: {e}")

if __name__ == "__main__":
    main()