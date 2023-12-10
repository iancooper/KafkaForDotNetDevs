// See https://aka.ms/new-console-template for more information

using Spectre.Console;
using Transmogrification;

if (!AnsiConsole.Profile.Capabilities.Interactive)
{
    AnsiConsole.MarkupLine("[red]Environment does not support interaction.[/]");
    return;
}

var theDial = new Dial();

// Confirmation
if (!theDial.AskConfirmation())
{
    return;
}

// Ask the user for settings
var name = theDial.AskName();

theDial.WriteDivider("Dial");
var transformation = Dial.AskTransformation(name);

//Display the user's choices
AnsiConsole.WriteLine();
AnsiConsole.Write(new Rule("[yellow]Dial[/]").RuleStyle("grey").LeftJustified());
AnsiConsole.Write(new Table().AddColumns("[grey]Setting[/]", "[grey]Value[/]")
    .RoundedBorder()
    .BorderColor(Color.Grey)
    .AddRow("[grey]Name[/]", name)
    .AddRow("[grey]Transformation[/]", transformation));

//Tell the user to enter the transmogrifier
theDial.WriteDivider("Transmogrifier");
AnsiConsole.MarkupLine($"[grey]Enter the transmogrifier [yellow]{name}[/] ...[/]");
AnsiConsole.MarkupLine($"[grey]...and you'll come out as [yellow]{transformation}[/]![/]");

//Wait for the user to press a key
AnsiConsole.WriteLine();
AnsiConsole.Confirm("[grey]Press y to begin transfmogrification...[/]");


