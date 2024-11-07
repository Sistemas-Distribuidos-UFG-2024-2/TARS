## 🚧 Comandos 🚧

* Para executar localmente:

```
gcc internal_temperature_sensor.c -o internal_temperature_sensor -lrabbitmq
./internal_temperature_sensor
```

* Comandos para criar a imagem e container Docker:

> O usuário e senha podem variar

```
docker build -t internal-temperature-sensor .
docker run -d -e RABBITMQ_USER=guest -e RABBITMQ_PASSWORD=guest --name internal-temperature-sensor internal-temperature-sensor
```

