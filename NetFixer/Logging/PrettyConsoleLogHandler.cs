using NetFixer.Interfaces;
using Spectre.Console;

namespace NetFixer.Logging
{
    public class PrettyConsoleLogHandler : ILog
    {
        public void Info(string message)
        {
            AnsiConsole.MarkupLine($"[grey][[INFO]][/] {message}");
        }

        public void Success(string message)
        {
            AnsiConsole.MarkupLine($"[green][[SUCCESS]] {message}[/]");
            AnsiConsole.MarkupLine("");
        }

        public void Error(string message)
        {
            AnsiConsole.MarkupLine($"[red][[ERROR]] {message}[/]");
        }

        public void Command(string command, string output, string error, int exitCode)
        {
            AnsiConsole.MarkupLine($"\n[mediumpurple1][[COMMAND]] {command}[/]");

            //var result = "Успешно";

            //if (exitCode == 1)
            //{
            //    result = "Ошибка";
            //    AnsiConsole.MarkupLine($"[red][[RESULT]] {result}[/]");
            //}
            //else
            //{
            //    AnsiConsole.MarkupLine($"[green][[RESULT]] {result}[/]");
            //}

            if (!string.IsNullOrWhiteSpace(output))
            {
                var lines = output.Split(
                    new[] { "\r\n", "\n" },
                    StringSplitOptions.RemoveEmptyEntries
                );

                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        AnsiConsole.MarkupLine($"[yellow][[STDOUT]] {Markup.Escape(line)}[/]");
                    }
                }
            }

            //if (!string.IsNullOrWhiteSpace(output))
            //    AnsiConsole.MarkupLine($"[white][[STDOUT]] {Markup.Escape(output)}[/]");
            if (!string.IsNullOrWhiteSpace(error))
                AnsiConsole.MarkupLine($"[red][[STDERR]] {Markup.Escape(error)}[/]");
        }
    }
}
