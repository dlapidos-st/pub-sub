# PubSub Example


## Purpose
This project demonstrates the use of internal PubSub components to produce new messages into a Kafka topic, and to consume new events written to the same Kafka topic.

The docker-compose file creates a simple Kafka multi-node cluster, as well as KafDrop (a simple Kafka admin tool).

## Requirements
- Docker
- .Net 6
- Visual Studio 2022

## Trying it out:
- Start the Kafka cluster:
    ```
    docker-compose up --build -d
    ```
- Start the application
- To create new messages: submit a `POST` request to http://localhost:5175/message?count=10 (the `count` query string parameter indicates the number of messages to create)
- By default, consumption of messages will produce logs in the console window. To prevent consumption of messages, comment out the call to `.Consume()` in `Program.cs`
- To stop the cluster and remove all Kafka files, execute `docker-compose down -v`, or to stop the cluster without destroying Kafka files, execute `docker-compose stop`
