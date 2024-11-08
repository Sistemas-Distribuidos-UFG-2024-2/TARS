## 🚧 Comandos 🚧

* Para executar localmente:

```
gcc acceleration_sensor.c -o acceleration_sensor -lrabbitmq
./acceleration_sensor
```

* Comandos para criar a imagem e container Docker:

> O usuário e senha podem variar

```
docker build -t acceleration-sensor .
docker run -d -e RABBITMQ_USER=guest -e RABBITMQ_PASSWORD=guest -e RABBITMQ_HOSTNAME=host.docker.internal --name acceleration-sensor acceleration-sensor
```

