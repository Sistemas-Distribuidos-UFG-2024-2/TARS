import java.io.*
import java.net.*

fun main() {

    val receiverAddress = "127.0.0.1" 
    val receiverPort = 4000 

    try {

        print("Enter your age: ")
        val ageInput = readLine()?.trim()
        val age = ageInput?.toIntOrNull()
        if(age == null || age < 5) {
            println("Invalid age. Enter a valid positive number greater than or equal to 5 years old.")
            return
        }

        Socket(receiverAddress, receiverPort).use { socket ->
            val outputWriter = PrintWriter(socket.getOutputStream())
            outputWriter.print(age) 
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