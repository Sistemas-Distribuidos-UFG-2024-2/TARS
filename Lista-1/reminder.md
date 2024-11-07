## 🚧 Comandos 🚧

> Importante ter o compilador do Kotlin (kotlinc) e Python3 instalados

### Versão em Kotlin e Python

* Sockets

    ```
    python3 receiver.py
    kotlinc sender.kt -include-runtime -d sender.jar
    java -jar sender.jar
    ```

* RPC

    ```
    python3 receiver.py
    kotlinc sender.kt -cp libs/xmlrpc-client-3.1.3.jar:libs/commons-logging-1.2.jar:libs/xmlrpc-common-3.1.3.jar -d sender.jar
    kotlin -cp libs/xmlrpc-client-3.1.3.jar:libs/commons-logging-1.2.jar:libs/xmlrpc-common-3.1.3.jar:sender.jar SenderKt
    ```

### Versão somente em Python

* RMI

> O primeiro comando é responsável por iniciar o servidor de nomes do Pyro5

    ```
    python3 -m Pyro5.nameserver
    python3 receiver.py
    python3 sender.py
    ```