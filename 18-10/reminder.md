## 🚧 Comandos 🚧

### Versão em Kotlin e Python

> Importante ter o compilador do Kotlin instalado (kotlinc)

* Sockets

    ```
    python3 receiver.py
    kotlinc sender.kt -include-runtime -d sender.jar
    java -jar sender.jar
    ```

* RCP

    ```
    python3 receiver.py
    kotlinc sender.kt -cp libs/xmlrpc-client-3.1.3.jar:libs/commons-logging-1.2.jar:libs/xmlrpc-common-3.1.3.jar -d sender.jar
    kotlin -cp libs/xmlrpc-client-3.1.3.jar:libs/commons-logging-1.2.jar:libs/xmlrpc-common-3.1.3.jar:sender.jar SenderKt
    ```