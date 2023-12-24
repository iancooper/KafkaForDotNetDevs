# we want to execute some commands on our kafka server
docker container exec -it kafka0 /bin/bash

# add a new topic
cd /bin
kafka-topics --create --if-not-exists --topic test-topic --partitions 3 --bootstrap-server localhost:9092

#use kafka cli
kafka-console-producer --broker-list localhost:9092 --topic test-topic
{"name":"Coops"}

kafka-console-consumer --bootstrap-server localhost:9092 --topic test-topic -from-beginning

# exit from Docker, and back to the host terminal session
#use kcat

kcat -b localhost:9092 -t test-topic
kcat -b localhost:9092 -t test-topic -P
{"name":"Ian"}

#terminate the producer
[CTRL+D]

