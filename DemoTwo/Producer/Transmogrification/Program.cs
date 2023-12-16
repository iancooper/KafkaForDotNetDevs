// See https://aka.ms/new-console-template for more information

using Transmogrification;

const string topic = "transmogrification";

var producerConfig = new Dictionary<string, string>
{
    { "bootstrap.servers", "localhost:9092" }
};

var theBox = new Box();
var theDial = new Dial();

var settings = new TransmogrificationSettings();
settings.Name = theBox.AskName(); 

settings.Transformation = theDial.AskForTransformation(settings.Name);

theDial.DisplaySettings(settings);

theBox.EnterTransmogrifier(settings);

var dispatcher = new Dispatcher(topic, producerConfig);
dispatcher.Transmogrify(settings);









