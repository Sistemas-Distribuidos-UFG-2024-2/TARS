### Comandos - Versão em Kotlin e Python

> ⚠️ Importante ter o compilador do Kotlin instalado (kotlinc)

1. Sockets

    ```
    python3 receiver.py
    kotlinc sender.kt -include-runtime -d sender.jar
    java -jar sender.jar
    ```