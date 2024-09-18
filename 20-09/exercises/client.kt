import java.io.*
import java.net.*

fun main() {

    /**
        Novo: serviço de health check, balanceador e servidores multi-threaded
    */

    val balancerAddress = "127.0.0.1" // Endereço IP do balanceador de carga
    val balancerPort = 4000 // Porta na qual o balanceador está escutando

    try {

        print("Digite sua altura (em metros): ")
        val height = readLine()?.trim()
        print("Digite seu sexo (F/M): ")
        val gender = readLine()?.trim()

        if(height.isNullOrEmpty() || gender.isNullOrEmpty()) {
            println("Por favor, preencha todos os campos.")
            return
        } 

        // Formata a mensagem para ficar no formato altura:sexo
        val message = "$height:$gender"

        // Cria uma conexão TCP entre o cliente e o balanceador de carga
        // Garante que o socket será fechado automaticamente após o bloco de código ser executado, mesmo se ocorrer uma exceção
        // Pega o socket recém-criado como argumento de entrada da lambda e usa esse socket dentro do bloco de código da lambda para enviar e receber dados
        Socket(balancerAddress, balancerPort).use { socket ->

            // Permite que o cliente envie mensagens ao balanceador pelo 'fluxo' de saída do socket
            val outputWriter = PrintWriter(socket.getOutputStream())
            outputWriter.print(message) // Escreve a mensagem no socket
            outputWriter.flush()  // Garante que os dados sejam enviados imediatamente

            // Permite que o cliente leia as mensagens recebidas do balanceador pelo 'fluxo' de entrada do socket
            val inputReader = BufferedReader(InputStreamReader(socket.getInputStream()))
            val response = inputReader.readLine() // Lê a mensagem do socket
            println("Response from server: $response")

        }

    } catch (e: IOException) {
        // Captura qualquer erro encontrado durante o processo de conexão ou comunicação, como se estiver offline ou ocupado
        e.printStackTrace()
        println("Failed to connect to the load balancer.")
    }
}