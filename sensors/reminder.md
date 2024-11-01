## 🚧 Comandos 🚧

* Para executar localmente:

```
gcc temperature_sensor.c -o temperature_sensor -lrabbitmq
./temperature_sensor
```

* Comandos para criar a imagem e container Docker:

> O usuário e senha podem variar

```
docker build -t external-temperature-sensor .
docker run -d -e RABBITMQ_USER=guest -e RABBITMQ_PASSWORD=guest --name external-temperature-sensor external-temperature-sensor
```

