FROM frolvlad/alpine-gcc:latest

# Instalar as dependências necessárias para compilar a librabbitmq
RUN apk add --no-cache cmake make git openssl-dev && \
    # Baixar e compilar a librabbitmq
    git clone https://github.com/alanxz/rabbitmq-c.git && \
    cd rabbitmq-c && \
    mkdir build && \
    cd build && \
    cmake .. && \
    make && \
    make install && \
    cd ../.. && \
    rm -rf rabbitmq-c && \
    # Limpar pacotes de desenvolvimento para reduzir o tamanho da imagem
    apk del cmake make git openssl-dev

# Definir o diretório de trabalho dentro do contêiner
WORKDIR /app

ENV RABBITMQ_USER=guest
ENV RABBITMQ_PASSWORD=guest

# Copiar código-fonte e outros arquivos necessários para o contêiner
COPY sensor.c .
COPY spacecraft_internal_pressure_value.txt .

# Compilação do programa
RUN gcc sensor.c -o sensor -lrabbitmq

# Definir o comando para execução do contêiner, permitindo que parâmetros sejam passados
ENTRYPOINT ["./sensor"]
CMD []  # Permite passar argumentos para o sensor, se necessário

