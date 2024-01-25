using Spectre.Console;

namespace Transmogrification;

public class Box
{
    public string AskName()
    {
        return AnsiConsole.Ask<string>("What's your [green]name[/]?");
    }
    
    public void EnterTransmogrifier(TransmogrificationSettings settings)
    {
        WriteDivider("Transmogrifier");
        AnsiConsole.MarkupLine($"[grey]Enter the transmogrifier [yellow]{settings.Name}[/] ...[/]");
        AnsiConsole.MarkupLine($"[grey]...and you'll come out as [yellow]{settings.Transformation}[/]![/]");
    }
    
    private void WriteDivider(string text)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[yellow]{text}[/]").RuleStyle("grey").LeftJustified());
    }

    public bool AskIfDone()
    {
        return AnsiConsole.Confirm("Are you [green]done[/]?");
    }
}