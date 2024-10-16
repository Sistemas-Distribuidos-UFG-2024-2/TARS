import java.io.OutputStreamWriter
import java.net.HttpURLConnection
import java.net.URL
import javax.xml.parsers.DocumentBuilderFactory
import org.w3c.dom.Element

fun main() {
    try {

        val serverUrl = "http://localhost:5000"

        print("Enter your age: ")
        val ageInput = readLine()?.trim()
        val age = ageInput?.toIntOrNull()
        if(age == null || age < 5) {
            println("Invalid age. Enter a valid positive number greater than or equal to 5 years old.")
            return
        }

        val xmlRequest = """
            <?xml version="1.0"?>
            <methodCall>
                <methodName>define_category</methodName>
                <params>
                    <param><value><int>$age</int></value></param>
                </params>
            </methodCall>
        """.trimIndent()

        // Configurar a conexão HTTP para enviar o XML ao servidor
        val url = URL(serverUrl)
        val connection = url.openConnection() as HttpURLConnection
        connection.requestMethod = "POST"
        connection.setRequestProperty("Content-Type", "text/xml")
        connection.doOutput = true

        // Enviar a requisição com o XML para o servidor
        val outputStream = OutputStreamWriter(connection.outputStream)
        outputStream.write(xmlRequest)
        outputStream.flush() 
        outputStream.close()

        // Ler a resposta do servidor (que também é um XML)
        val responseStream = connection.inputStream
        val document = DocumentBuilderFactory.newInstance().newDocumentBuilder().parse(responseStream)
        document.documentElement.normalize()

        // Extração da informação do XML de resposta (navegar na estrutura XML procurando info)
        val valueElement = document.getElementsByTagName("string").item(0) as Element
        val category = valueElement.textContent
        println("Category: $category")

    } catch (e: Exception) {
        println("Failed to connect to the receiver.")
        e.printStackTrace()
    }
}