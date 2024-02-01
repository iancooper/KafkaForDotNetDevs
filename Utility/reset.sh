# we want to execute some commands on our kafka server
docker container exec -it kafka0 /bin/bash

# add a new topic
cd /bin

kafka-consumer-groups --bootstrap-server localhost:9092 --group transmogrification-consumer --reset-offsets --to-earliest --topic transmogrification -execute
