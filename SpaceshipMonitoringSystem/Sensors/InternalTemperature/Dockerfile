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
COPY internal_temperature_sensor.c .
COPY internal_temperatures.txt .

# Compilação
RUN gcc internal_temperature_sensor.c -o internal_temperature_sensor -lrabbitmq

# Execução
ENTRYPOINT ["./internal_temperature_sensor"]