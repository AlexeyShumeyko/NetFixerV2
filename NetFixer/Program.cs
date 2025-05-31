using NetFixer.UI;

Console.Title = "NetFix Tool";
Console.OutputEncoding = System.Text.Encoding.UTF8;

await ConsoleUI.RunAutomatedDiagnostics();