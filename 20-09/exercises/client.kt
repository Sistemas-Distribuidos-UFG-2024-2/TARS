import java.io.*
import java.net.*

fun main() {

    /**
        Novo: serviço de health check multi-threaded, balanceador e servidores multi-threaded
        Fluxo: Cliente -> Balanceador -> Health Check -> Balanceador -> Servidor -> Balanceador -> Cliente

        Cliente -> Balanceador: O cliente envia uma mensagem ao balanceador.
        Balanceador -> Health Check: O balanceador se comunica com o serviço de health check para verificar quais servidores estão disponíveis.
        Balanceador -> Servidor: O balanceador encaminha a mensagem do cliente para um servidor disponível, que processa a requisição.
        Servidor -> Balanceador -> Cliente: O servidor retorna a resposta ao balanceador, que a encaminha de volta ao cliente.
    */

    val balancerAddress = "127.0.0.1" // Endereço IP do balanceador de carga
    val balancerPort = 4000 // Porta na qual o balanceador está escutando

    try {

        // trim(): Remove espaços em branco no início ou final
        print("Enter your height (in meters): ")
        val heightInput = readLine()?.trim()

        // Tenta converter a string de altura para float, se não der certo retorna null
        val height = heightInput?.toFloatOrNull()
        if(height == null || height <= 0) {
            println("Invalid height. Enter a valid positive number.")
            return
        }

        print("Enter your gender (F/M): ")
        val gender = readLine()?.trim()?.uppercase()

        if(gender.isNullOrEmpty() || (gender != "M" && gender != "F")) {
            println("Invalid gender. Use M for male or F for female.")
            return
        }

        // Formata a mensagem para ficar no formato 'altura:sexo'
        val message = "$height:$gender"

        /** Cria uma conexão TCP entre o cliente e o balanceador de carga
            Garante que o socket será fechado automaticamente após o bloco de código ser executado, mesmo se ocorrer uma exceção
            Pega o socket recém-criado como argumento de entrada da lambda e usa esse socket dentro do bloco de código da lambda para enviar e receber dados
        */

        Socket(balancerAddress, balancerPort).use { socket ->

            // Permite que o cliente envie mensagens ao balanceador pelo 'fluxo' de saída do socket
            val outputWriter = PrintWriter(socket.getOutputStream())
            outputWriter.print(message) // Escreve a mensagem no socket
            outputWriter.flush()  // Garante que os dados sejam enviados imediatamente

            // Permite que o cliente leia a resposta do servidor, transmitida pelo balanceador, através do 'fluxo' de entrada do socket
            val inputReader = BufferedReader(InputStreamReader(socket.getInputStream()))
            val response = inputReader.readLine() // Lê a mensagem do socket
            println("Response from server: $response")

        }

    } catch (e: IOException) {
        // Captura qualquer erro encontrado durante o processo de conexão ou comunicação
        e.printStackTrace()
        println("Failed to connect to the load balancer.")
    }
}