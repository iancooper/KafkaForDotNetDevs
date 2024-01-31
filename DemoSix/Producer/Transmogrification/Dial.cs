using System.Globalization;
using Confluent.SchemaRegistry;
using Spectre.Console;

namespace Transmogrification;

public class Dial
{
    public string AskForTransformation(string selfName)
    {
        WriteDivider("Dial");
        return AnsiConsole.Prompt(
            new TextPrompt<string>("Which [green]transformation[/]?")
                .InvalidChoiceMessage("[red]That's not a supported transformation![/]")
                .DefaultValue("Transformation?")
                .AddChoice(selfName)
                .AddChoice("Tiger")
                .AddChoice("Eel")
                .AddChoice("Baboon")
                .AddChoice("Bug")
                .AddChoice("Dinosaur")
                .AddChoice("Frog")
                .AddChoice("Worm")
                );
    }
    
    public void DisplaySettings(TransmogrificationSettings settings)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[yellow]Dial[/]").RuleStyle("grey").LeftJustified());
        AnsiConsole.Write(new Table().AddColumns("[grey]Setting[/]", "[grey]Value[/]")
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .AddRow("[grey]Name[/]", settings.Name)
            .AddRow("[grey]Transformation[/]", settings.Transformation));
    }

    private void WriteDivider(string text)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[yellow]{text}[/]").RuleStyle("grey").LeftJustified());
    }

    public void DisplaySentOrLostMessages(IOutbox outbox, string topic)
    {
        var persisted = outbox.FindPersisted(topic).ToList();
        WriteDivider("Sent Messages");
        var sentMessages = new Table()
            .AddColumns("[grey]Topic[/]", "[grey]Key[/]", "[grey]Value[/]", "[grey]Partition[/]", "[grey]Status[/]", "[grey]Timestamp[/]")
            .RoundedBorder()
            .BorderColor(Color.Grey);
        
        foreach (var entry in persisted)
        {
            sentMessages.AddRow(entry.Topic, entry.Key, entry.Value.Value.Transformation, entry.Partition.Value.ToString(), 
                entry.Status.ToString(), entry.Timestamp.UtcDateTime.ToString(CultureInfo.InvariantCulture));
        }
        
        AnsiConsole.Write(sentMessages);
        
        var notPersisted = outbox.FindNotPersisted(topic).ToList();
        WriteDivider("Lost Messages");
        var lostMessages = new Table().AddColumns("[grey]Topic[/]", "[grey]Key[/]", "[grey]Value[/]")
            .RoundedBorder()
            .BorderColor(Color.Grey);
        
        foreach (var entry in notPersisted)
        {
            lostMessages.AddRow(entry.Topic, entry.Key, entry.Value.Value.Transformation);
        }
        
        AnsiConsole.Write(lostMessages);
    }

    public void DisplaySchemaOfSentMessage(SchemaRegistryConfig schemaRegistryConfig, string topicName)
    {
        WriteDivider("Schema");
        AnsiConsole.WriteLine("The JSON schema corresponding to the written data:");
        using (var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig))
        {
            //assumes the default subject name strategy of topic name + "-value"
            var schema = schemaRegistry.GetLatestSchemaAsync(SubjectNameStrategy.Topic.ConstructValueSubjectName(topicName)).Result;
            AnsiConsole.WriteLine("The JSON schema corresponding to the written data:");
            AnsiConsole.WriteLine(schema.SchemaString);
        }
    }
}