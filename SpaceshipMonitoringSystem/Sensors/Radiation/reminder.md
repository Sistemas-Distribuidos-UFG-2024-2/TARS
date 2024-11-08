## 🚧 Comandos 🚧

* Para executar localmente:

```
gcc radiation_sensor.c -o radiation_sensor -lrabbitmq
./radiation_sensor
```

* Comandos para criar a imagem e container Docker:

> O usuário e senha podem variar

```
docker build -t radiation-sensor .
docker run -d -e RABBITMQ_USER=guest -e RABBITMQ_PASSWORD=guest RABBITMQ_HOSTNAME=host.docker.internal --name radiation-sensor radiation-sensor
```

