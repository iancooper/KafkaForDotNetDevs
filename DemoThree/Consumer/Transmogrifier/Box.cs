using Spectre.Console;

namespace Transmogrifier;

public class Box
{
    public bool StarTransformation()
    {
        AnsiConsole.WriteLine();
        return AnsiConsole.Confirm("[green]Begin Transfmogrification?[/]");
    }

    public void BeginTransforming()
    {
        WriteDivider("Transformations");
    }
    
    private void WriteDivider(string text)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[yellow]{text}[/]").RuleStyle("grey").LeftJustified());
    }

    public void EndTransforming()
    {
        WriteDivider("End of Transformations");
    }
}