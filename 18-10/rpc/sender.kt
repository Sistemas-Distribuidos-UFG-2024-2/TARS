import java.io.OutputStreamWriter
import java.net.HttpURLConnection
import java.net.URL
import javax.xml.parsers.DocumentBuilderFactory
import org.w3c.dom.Element

fun main() {
    try {
        // Endereço do servidor XML-RPC
        val serverUrl = "http://localhost:5000"

        // Solicitar altura e gênero do usuário
        print("Enter your height (in meters): ")
        val heightInput = readLine()?.trim()
        val height = heightInput?.toDoubleOrNull()
        if (height == null || height < 1.0) {
            println("Invalid height. Enter a valid positive number greater than or equal to 1 meter.")
            return
        }

        print("Enter your gender (F/M): ")
        val gender = readLine()?.trim()?.uppercase()
        if (gender.isNullOrEmpty() || (gender != "M" && gender != "F")) {
            println("Invalid gender. Use M for male or F for female.")
            return
        }

        // Construir a solicitação XML-RPC
        val xmlRequest = """
            <?xml version="1.0"?>
            <methodCall>
                <methodName>define_ideal_weight</methodName>
                <params>
                    <param><value><double>$height</double></value></param>
                    <param><value><string>$gender</string></value></param>
                </params>
            </methodCall>
        """.trimIndent()

        // Configurar a conexão HTTP
        val url = URL(serverUrl)
        val connection = url.openConnection() as HttpURLConnection
        connection.requestMethod = "POST"
        connection.setRequestProperty("Content-Type", "text/xml")
        connection.doOutput = true

        // Enviar a solicitação
        val outputStream = OutputStreamWriter(connection.outputStream)
        outputStream.write(xmlRequest)
        outputStream.flush()
        outputStream.close()

        // Ler a resposta do servidor
        val responseStream = connection.inputStream
        val document = DocumentBuilderFactory.newInstance().newDocumentBuilder().parse(responseStream)
        document.documentElement.normalize()

        val valueElement = document.getElementsByTagName("double").item(0) as Element
        val idealWeight = valueElement.textContent.toDouble()

        println("Ideal weight: %.2f kg".format(idealWeight))

    } catch (e: Exception) {
        e.printStackTrace()
        println("Failed to connect to the receiver.")
    }
}