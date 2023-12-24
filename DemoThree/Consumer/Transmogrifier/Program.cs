// See https://aka.ms/new-console-template for more information

using Confluent.Kafka;
using Spectre.Console;
using Transmogrifier;

const string topic = "transmogrification";

var consumerConfig = new ConsumerConfig()
{
    BootstrapServers = "localhost:9092",
    GroupId = "transmogrification-consumer",
    AutoOffsetReset = AutoOffsetReset.Earliest,
    EnableAutoCommit = true,
    EnableAutoOffsetStore = false
};

CancellationTokenSource cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => {
    e.Cancel = true; // prevent the process from terminating.
    cts.Cancel();
};

var box = new Box();
if (box.StarTransformation())
{
    box.BeginTransforming();
    
    var messagePump = new MessagePump(topic, consumerConfig);
    await messagePump.Run(
        (message) => new Transmogrification(message.Key, message.Value),
        (transmogrification) =>
        {
            //Output the results using a spectre console table
            var table = new Table();
            table.AddColumn("Name");
            table.AddColumn("Transformation");
            table.AddColumn("Result");

            table.AddRow(transmogrification.From, transmogrification.To, "Success");
        
            AnsiConsole.Write(table);

            return new HandleResult(true);

        }, 
    cts.Token);
    
    box.EndTransforming();
}



