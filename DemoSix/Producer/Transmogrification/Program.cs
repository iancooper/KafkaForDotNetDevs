// See https://aka.ms/new-console-template for more information

using System.Net;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Transmogrification;

const string topic = "transmogrification";

var producerConfig = new ProducerConfig
{
    BootstrapServers = "localhost:9092",
    EnableDeliveryReports = true,
    ClientId = Dns.GetHostName(),
    Acks = Acks.All,
    EnableIdempotence = true, //de-duplication, and ordering requires Acks.All and sets MaxInFlight to 5, MessageSendMaxRetries to MAX_INT
    TransactionalId = "transmogrification-1",
    TransactionTimeoutMs = 30000,
    //MessageSendMaxRetries = int.MaxValue
    //MaxInFlight = 5,
};

var schemaRegistryConfig = new SchemaRegistryConfig
{
    Url = "http://localhost:8085"
};

var theBox = new Box();
var theDial = new Dial();

//a real outbox would be persistent and would be able to survive a restart of the application
var outbox = new InMemoryOutbox(new []{topic});

var dispatcher = new Dispatcher(topic, producerConfig, schemaRegistryConfig, outbox);

bool stop = false;

while (!stop)
{

    var settings = new TransmogrificationSettings();
    settings.Name = theBox.AskName();

    settings.Transformation = theDial.AskForTransformation(settings.Name);

    theDial.DisplaySettings(settings);

    theBox.EnterTransmogrifier(settings);

    dispatcher.Transmogrify(settings);
    
    stop = theBox.AskIfDone();
}

theDial.DisplaySentOrLostMessages(outbox, topic);









