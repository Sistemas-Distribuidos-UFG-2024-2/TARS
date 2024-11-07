import java.io.*
import java.net.*

fun main() {

    val receiverAddress = "127.0.0.1" 
    val receiverPort = 4000 

    try {

        print("Enter your average balance: ")
        val balanceInput = readLine()?.trim()
        val balance = balanceInput?.toDoubleOrNull()
        if(balance == null || balance < 0) {
            println("Invalid balance. Enter a valid positive number greater than or equal to zero.")
            return
        }

        Socket(receiverAddress, receiverPort).use { socket ->
            val outputWriter = PrintWriter(socket.getOutputStream())
            outputWriter.print(balance) 
            outputWriter.flush()
            val inputReader = BufferedReader(InputStreamReader(socket.getInputStream()))
            val response = inputReader.readLine() 
            println("Response from receiver: $response")

        }

    } catch (e: IOException) {
        e.printStackTrace()
        println("Failed to connect to the receiver.")
    }
}