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

}