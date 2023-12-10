using Spectre.Console;

namespace Transmogrification;

public class Dial
{
    public bool AskConfirmation()
    {
        if (!AnsiConsole.Confirm("Run Transmogrifier?"))
        {
            AnsiConsole.MarkupLine("Ok... :(");
            return false;
        }

        return true;
    }

    public string AskName()
    {
        var name = AnsiConsole.Ask<string>("What's your [green]name[/]?");
        return name;
    }
    
    public static string AskTransformation(string selfName)
    {
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

    public void WriteDivider(string text)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[yellow]{text}[/]").RuleStyle("grey").LeftJustified());
    }

}