## 🚧 Comandos 🚧

* Para executar localmente:

```
gcc fuel_pressure_sensor.c -o fuel_pressure_sensor -lrabbitmq
./fuel_pressure_sensor
```

* Comandos para criar a imagem e container Docker:

> O usuário e senha podem variar

```
docker build -t fuel-pressure-sensor .
docker run -d -e RABBITMQ_USER=guest -e RABBITMQ_PASSWORD=guest --name fuel-pressure-sensor fuel-pressure-sensor
```

