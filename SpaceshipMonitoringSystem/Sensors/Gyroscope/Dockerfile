#Cria uma imagem de base com as ferramentas necessárias para rodar o código
#Nesse caso é o gcc para rodar o programa em C
FROM frolvlad/alpine-gcc:latest

# Instala as dependências para o RabbitMQ C Client Library (librabbitmq)
RUN apk add --no-cache cmake make git openssl-dev

#Cria um diretório de trabalho no container
WORKDIR /app

# Baixa e instala a librabbitmq (RabbitMQ C Client)
RUN git clone https://github.com/alanxz/rabbitmq-c.git && \
    cd rabbitmq-c && \
    mkdir build && \
    cd build && \
    cmake .. && \
    make && \
    make install && \
    cd ../.. && \
    rm -rf rabbitmq-c

#Copia o código fonte do sensor e os dados para o diretório de trabalho
COPY gyroscope_sensor.c .
COPY gyroscope_values.txt .

#Compila a cópia gerada previamente dentro do diretório
RUN gcc gyroscope_sensor.c -o gyroscope_sensor -lrabbitmq

#Define o comando padrão para rodar o executável
ENTRYPOINT ["./gyroscope_sensor"]
