// See https://aka.ms/new-console-template for more information

using Spectre.Console;
using Transmogrifier;

const string topic = "transmogrification";

var consumerConfig = new Dictionary<string, string>()
{
    { "bootstrap.servers", "localhost:9092" },
    { "group.id", "transmogrification-consumer" },
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

        }, 
    cts.Token);
    
    box.EndTransforming();
}



