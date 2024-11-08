## 🚧 Comandos 🚧

* Para executar localmente:

```
gcc gyroscope_sensor.c -o gyroscope_sensor -lrabbitmq
./gyroscope_sensor
```

* Comandos para criar a imagem e container Docker:

> O usuário e senha podem variar

```
docker build -t gyroscope-sensor .
docker run -d -e RABBITMQ_USER=guest -e RABBITMQ_PASSWORD=guest RABBITMQ_HOSTNAME=host.docker.internal --name gysroscope-sensor gyroscope-sensor
```

