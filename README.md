### 📌 Integrantes do Grupo TARS

* Hafy Mourad Jacoub de Cuba Kouzak
* Gustavo Neves Piedade Louzada
* Igor Rodrigues Castilho
* João Victor de Paiva Albuquerque
* Maria Eduarda de Campos Ramos

### ⚙️ [Projeto Final] Spaceship Monitoring System

Para rodar o projeto final, acesse a pasta raíz dele e execute o seguinte comando:
```
docker-compose up --build -d
```
> Obs.: É necessário ter o [Docker](https://www.docker.com/) instalado.

![Arquitetura do Projeto](https://github.com/user-attachments/assets/6b185b1b-df7e-452a-9718-18a35f67d07e)

**Resumo:** Os diversos tipos de sensores acoplados à nave espacial enviam dados continuamente para a interface da nave através de comunicação direta (sockets) e publicam essas informações no RabbitMQ simultaneamente. A interface da nave armazena os dados em um arquivo txt dentro de uma pasta chamada "Logs". No RabbitMQ, os serviços de análise de dados consomem essas informações para identificar possíveis anomalias e as salvam no banco de dados. Caso uma anomalia seja detectada, os serviços de análise publicam alertas no RabbitMQ, que são consumidos pelo Houston (serviço de controle central), pela interface da nave e pelo serviço de notificação. Este último é responsável por enviar os alertas por e-mail aos usuários cadastrados no banco de dados. Além disso, o Houston e a interface da nave podem se comunicar através da publicação e consumo de mensagens no RabbitMQ. Por fim, o Houston pode acessar o histórico de dados de cada sensor armazenado no banco de dados.

**Tecnologias e Linguagens utilizadas:**
* Sensores: Desenvolvidos em C
* Broker de mensagens: RabbitMQ
* Serviços de análise de dados, de notificação, Houston e interface da nave espacial: Desenvolvidos em C#
* Banco de dados: MongoDB
* Docker

**Endpoints:**
* Cadastrar um usuário no banco de dados (POST): ```http://localhost:5166/api/persons```

  Exemplo:
    ```
    {
      "name": "Maria Eduarda",
      "email": "maria_campos@discente.ufg.br"
    }
    ```
  
* Enviar mensagem do Houston para a interface da nave espacial (POST): ```http://localhost:5008/api/spaceship```

  Exemplo:
  ```
  {
    "text": "Mensagem do Houston para a Espaçonave!"
  }
  ```

* Enviar mensagem da interface da nave espacial para o Houston (POST): ```http://localhost:5236/api/houston```

  Exemplo:
  ```
  {
    "text": "Mensagem da Espaçonave para o Houston!"
  }
  ```

* Resgatar dados dos sensores no Houston (GET):
    * Aceleração - ```http://localhost:5008/api/sensors/acceleration```
    * Temperatura externa - ```http://localhost:5008/api/sensors/external-temperature```
    * Pressão de combustível - ```http://localhost:5008/api/sensors/fuel-pressure```
    * Giroscópico - ```http://localhost:5008/api/sensors/gyroscope```
    * Pressão interna - ```http://localhost:5008/api/sensors/internal-pressure```
    * Temperatura interna - ```http://localhost:5008/api/sensors/internal-temperature```
    * Radiação - ```http://localhost:5008/api/sensors/radiation```

<br>

> ⚠️ Mais detalhes sobre o projeto estão na pasta "slides" deste repositório.
