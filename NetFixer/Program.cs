using NetFixer.UI;
using NetFixer.Utils;

Console.Title = "NetFix Tool";
Console.OutputEncoding = System.Text.Encoding.UTF8;

if (OsVersionChecker.IsWindows7())
{
    ConsoleUI.ShowWindows7Warning();
    return;
}

if (OsVersionChecker.IsMacOS())
{
    ConsoleUI.ShowMacOsWarning();
    return;
}

await ConsoleUI.RunAutomatedDiagnostics();