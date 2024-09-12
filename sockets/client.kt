import java.io.*
import java.net.*

fun main() {

    /**
        Abordagem: É como se os servidores fizessem parte de um conjunto de servidores e o cliente interage, na verdade, com esse conjunto.
        Os servidores compartilham a carga e são vistos pelo cliente como um conjunto único. Então, utilizarei um balanceador de carga.
        Este será responsável por distribuir as requisições entre os servidores do conjunto. O cliente se conecta ao balanceador, que por sua vez,
        escolhe um dos servidores do conjunto que esteja disponível para atender a requisição.
    */

    val balancerAddres = "127.0.0.1" // Endereço IP do balanceador de carga
    val balancerPort = 4000 // Porta na qual o balanceador está escutando
    val message = "Hello"

    try {

        // Cria uma conexão TCP entre o cliente e o balanceador de carga
        // Garante que o socket será fechado automaticamente após o bloco de código ser executado, mesmo se ocorrer uma exceção
        // Pega o socket recém criado como argumento de entrada da lambda e usa esse socket dentro do bloco para enviar e receber dados
        Socket(balancerAddres, balancerPort).use { socket ->

            // Permite que o cliente envie mensagens ao balanceador pelo 'fluxo' de saída do socket
            // Essa mensagem será enviada imediatamente após o print
            val outputWriter = PrintWriter(socket.getOutputStream(), true)
            outputWriter.print(message)
            outputWriter.flush()  // Garante que os dados sejam enviados imediatamente caso o buffer não esteja completamente cheio

            // Permite que o cliente leia as mensagens recebidas do balanceador pelo 'fluxo' de entrada do socket
            val inputReader = BufferedReader(InputStreamReader(socket.getInputStream()))
            val response = inputReader.readLine()
            println("Response from server: $response")

        }

    } catch (e: IOException) {
        // Captura qualquer erro encontrado durante o processo de conexão ou comunicação, como se estiver offline ou ocupado
        e.printStackTrace()
        println("Failed to connect to the load balancer.")
    }
}