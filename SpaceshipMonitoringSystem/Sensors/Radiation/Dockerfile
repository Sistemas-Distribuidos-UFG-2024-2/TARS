FROM frolvlad/alpine-gcc:latest

# Instala as dependências necessárias para baixar e compilar a biblioteca librabbitmq
RUN apk add --no-cache cmake make git openssl-dev

WORKDIR /app

# Baixa e compila a librabbitmq
RUN git clone https://github.com/alanxz/rabbitmq-c.git && \
    cd rabbitmq-c && \
    mkdir build && \
    cd build && \
    cmake .. && \
    make && \
    make install && \
    cd ../.. && \
    rm -rf rabbitmq-c

# Copia código do sensor e txt para o container
COPY radiation_sensor.c .
COPY radiation_values.txt .

# Compilação
RUN gcc radiation_sensor.c -o radiation_sensor -lrabbitmq

# Execução
ENTRYPOINT ["./radiation_sensor"]