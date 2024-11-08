## 🚧 Comandos 🚧

* Para executar localmente:

```
gcc external_temperature_sensor.c -o external_temperature_sensor -lrabbitmq
./external_temperature_sensor
```

* Comandos para criar a imagem e container Docker:

> O usuário e senha podem variar

```
docker build -t external-temperature-sensor .
docker run -d -e RABBITMQ_USER=guest -e RABBITMQ_PASSWORD=guest -e RABBITMQ_HOSTNAME=host.docker.internal --name external-temperature-sensor external-temperature-sensor
```

