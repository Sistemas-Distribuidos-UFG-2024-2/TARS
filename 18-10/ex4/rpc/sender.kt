// Imports para realizar conexões HTTP
import java.io.OutputStreamWriter
import java.net.HttpURLConnection
import java.net.URL
// Imports para manipular e analisar a resposta em XML
import javax.xml.parsers.DocumentBuilderFactory
import org.w3c.dom.Element

/**
 * Fluxo: O cliente coleta as informações do usuário, constrói o XML com essas info e indica qual o método a ser chamado.
 * Essa requisição é enviada para o servidor através de uma conexão HTTP. O servidor lê a requisição, executa o método 
 * correspondente e retorna uma resposta. O cliente lê a resposta e exibe o resultado.
 */

fun main() {
    try {

        // Endereço do servidor XML-RPC
        val serverUrl = "http://localhost:5000"

        // Coleta de dados do usuário
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

        // Construindo a requisição XML-RPC
        // .trimIndent() -> retirar os espaços em branco comuns antes de cada linha da string, mas mantendo a estrutura hierárquica/identação de um XML
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

        // Configurar a conexão HTTP para enviar o XML ao servidor

        // Cria um objeto URL
        val url = URL(serverUrl)
        // Abrir conexão HTTP com o servidor da URL
        // Casting para uma subclasse de URLConnection para enviar especificamente uma solicitação HTTP
        val connection = url.openConnection() as HttpURLConnection
        // Método HTTP usado na requisição (requestbody)
        connection.requestMethod = "POST"
        // Indica qual o tipo de dados que está sendo enviado no requestbody
        connection.setRequestProperty("Content-Type", "text/xml")
        // Indica que a conexão vai enviar dados ao servidor (POST), vai escrever no output stream da conexão
        connection.doOutput = true

        // Enviar a requisição com o XML para o servidor

        // Objeto OutputStreamWriter para escrever dados no fluxo de saída da conexão HTTP
        val outputStream = OutputStreamWriter(connection.outputStream)
        outputStream.write(xmlRequest)
        outputStream.flush() // Envio imediato dos dados
        outputStream.close() // Libera o fluxo de saída e fecha a conexão

        // Ler a resposta do servidor (que também é um XML)

        // Acesso ao fluxo de entrada associado à conexão
        val responseStream = connection.inputStream
        // Analisa o XML de resposta usando o DocumentBuilderFactory e o parse() para converter o XML em um DOM (Document Object Model), estrutura manipulável
        // O DOM é uma representação em árvore do documento XML, sendo mais fácil de navegar e extrair informações
        val document = DocumentBuilderFactory.newInstance().newDocumentBuilder().parse(responseStream)
        // Padronização da estrutura e evita problemas de espaçamento
        document.documentElement.normalize()

        // Extração da informação do XML de resposta (navegar na estrutura XML procurando info)

        // Acessa o primeiro elemento com a tag <double> do documento XML (como só tem um, fica mais fácil, basta pergar o primeiro)
        // Casting para element, acesso a método para manipular esse nó do XML
        val valueElement = document.getElementsByTagName("double").item(0) as Element
        // Extrai o valor/conteúdo de texto do elemento <double> e converte para Double
        val idealWeight = valueElement.textContent.toDouble()
        println("Ideal weight: %.2f kg".format(idealWeight))

    } catch (e: Exception) {
        println("Failed to connect to the receiver.")
        e.printStackTrace()
    }
}