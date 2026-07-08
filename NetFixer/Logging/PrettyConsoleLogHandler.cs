using NetFixer.Interfaces;
using Spectre.Console;

namespace NetFixer.Logging
{
    public class PrettyConsoleLogHandler : ILog
    {
        public void Info(string message)
        {
            AnsiConsole.MarkupLine($"[grey][[INFO]][/] {Markup.Escape(message)}");
        }

        public void Success(string message)
        {
            AnsiConsole.MarkupLine($"[green][[SUCCESS]] {Markup.Escape(message)}[/]");
            AnsiConsole.MarkupLine("");
        }

        public void Error(string message)
        {
            AnsiConsole.MarkupLine($"[red][[ERROR]] {Markup.Escape(message)}[/]");
        }

        public void Warning(string message)
        {
            AnsiConsole.MarkupLine($"[yellow][[WARNING]] {Markup.Escape(message)}[/]");
        }

        public void Group(string message)
        {
            AnsiConsole.MarkupLine($"[white][[GROUP]][/] {Markup.Escape(message)}");
        }

        public void StartPluginGroup(string message)
        {
            AnsiConsole.MarkupLine($"[grey][[START]][/] {Markup.Escape(message)}");
        }

        public void SubSection(string message)
        {
            AnsiConsole.MarkupLine($"[grey][[SECTION]][/] {Markup.Escape(message)}");
        }

        public void Command(string command, string output, string error, int exitCode)
        {
            AnsiConsole.MarkupLine($"\n[mediumpurple1][[COMMAND]] {Markup.Escape(command)}[/]");

            if (!string.IsNullOrWhiteSpace(output))
            {
                foreach (var line in output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                    AnsiConsole.MarkupLine($"[yellow][[STDOUT]] {Markup.Escape(line)}[/]");
            }

            if (!string.IsNullOrWhiteSpace(error))
                AnsiConsole.MarkupLine($"[red][[STDERR]] {Markup.Escape(error)}[/]");
        }

        public void Debug(string message)
        {
            AnsiConsole.MarkupLine($"[dim][[DEBUG]][/] {Markup.Escape(message)}");
        }
    }
}