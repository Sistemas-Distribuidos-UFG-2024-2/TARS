## 🚧 Comandos 🚧

* Para executar localmente:

```
gcc internal_pressure_sensor.c -o internal_pressure_sensor -lrabbitmq
./internal_pressure_sensor
```

* Comandos para criar a imagem e container Docker:

> O usuário e senha podem variar

```
docker build -t internal-pressure-sensor .
docker run -d -e RABBITMQ_USER=guest -e RABBITMQ_PASSWORD=guest --name internal-pressure-sensor internal-pressure-sensor
```