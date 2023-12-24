// See https://aka.ms/new-console-template for more information

using System.Net;
using Confluent.Kafka;
using Transmogrification;

const string topic = "transmogrification";

var producerConfig = new ProducerConfig
{
    BootstrapServers = "localhost:9092",
    EnableDeliveryReports = true,
    ClientId = Dns.GetHostName(),
    Acks = Acks.Leader,
    EnableIdempotence = true,
    MessageSendMaxRetries = 3,
    MaxInFlight = 1,
};

var theBox = new Box();
var theDial = new Dial();

var settings = new TransmogrificationSettings();
settings.Name = theBox.AskName(); 

settings.Transformation = theDial.AskForTransformation(settings.Name);

theDial.DisplaySettings(settings);

theBox.EnterTransmogrifier(settings);

var dispatcher = new Dispatcher(topic, producerConfig, new InMemoryOutbox(new []{topic}));
dispatcher.Transmogrify(settings);









