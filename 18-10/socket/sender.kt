import java.io.*
import java.net.*

fun main() {

    val receiverAddress = "127.0.0.1" 
    val receiverPort = 4000 // Porta na qual o receiver está escutando

    try {

        // trim(): Remove espaços em branco no início ou final
        print("Enter your height (in meters): ")
        val heightInput = readLine()?.trim()

        // Tenta converter a string de altura para float, se não der certo retorna null
        val height = heightInput?.toFloatOrNull()
        if(height == null || height < 1.0) {
            println("Invalid height. Enter a valid positive number greater than or equal to 1 meter.")
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

        /** Cria uma conexão TCP entre o sender e o receiver
            Garante que o socket será fechado automaticamente após o bloco de código ser executado, mesmo se ocorrer uma exceção
            Pega o socket recém-criado como argumento de entrada da lambda e usa esse socket dentro do bloco de código da lambda para enviar e receber dados
        */

        Socket(receiverAddress, receiverPort).use { socket ->

            // Permite que o sender envie mensagens ao receiver pelo 'fluxo' de saída do socket
            val outputWriter = PrintWriter(socket.getOutputStream())
            outputWriter.print(message) // Escreve a mensagem no socket
            outputWriter.flush()  // Garante que os dados sejam enviados imediatamente

            // Permite que o sender leia a resposta do receiver através do 'fluxo' de entrada do socket
            val inputReader = BufferedReader(InputStreamReader(socket.getInputStream()))
            val response = inputReader.readLine() // Lê a mensagem do socket
            println("Response from receiver: $response")

        }

    } catch (e: IOException) {
        // Captura qualquer erro encontrado durante o processo de conexão ou comunicação
        e.printStackTrace()
        println("Failed to connect to the load balancer.")
    }
}