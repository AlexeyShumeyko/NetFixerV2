using NetFixer.Core;
using NetFixer.Interfaces;
using NetFixer.Logging;
using Spectre.Console;

namespace NetFixer.UI
{
    public static class ConsoleUI
    {
        public static async Task ShowMainMenu()
        {
            var plugins = PluginManager.GetPlugins();
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                AnsiConsole.MarkupLine("[bold aqua]NetFix Tool[/]");
                AnsiConsole.MarkupLine("Пользуясь программой, вы соглашаетесь с условиями [underline green]Пользовательского соглашения[/].");
                AnsiConsole.WriteLine(new string('-', 100));

                var choices = new List<string>
                {
                    "Полная проверка (рекомендуется)"
                };

                choices.AddRange(plugins.Select(p => p.Name));

                choices.Add("Пользовательское соглашение");
                choices.Add("Выход");

                var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Выберите действие:")
                    .HighlightStyle("yellow")
                    .PageSize(10)
                    .AddChoices(choices)
                );

                if (choice == "Выход")
                    exit = true;
                else if (choice == "Пользовательское соглашение")
                    ShowAgreement();
                else if (choice == "Полная проверка (рекомендуется)")
                {
                    await RunPluginsSequentially(plugins);
                    AnsiConsole.WriteLine(new string('-', 100));
                }
                else
                {
                    var selectedPlugin = plugins.First(p => p.Name == choice);
                    await ExecutePlugin(selectedPlugin);
                    AnsiConsole.WriteLine(new string('-', 100));
                }

                AnsiConsole.MarkupLine("[grey]Нажмите любую клавишу для возврата в меню...[/]");
                Console.ReadKey(true);
            }
        }

        private static void ShowAgreement()
        {
            Console.Clear();

            if (File.Exists("Agreement.txt"))
                Console.WriteLine(File.ReadAllText("Agreement.txt"));
            else
                Console.WriteLine("Agreement.txt не найден.");
        }

        private static async Task RunPluginsSequentially(IEnumerable<INetFixPlugin> plugins)
        {
            var logger = new CombinedLogHandler(new PrettyConsoleLogHandler(), new FileLogHandler());

            foreach (var plugin in plugins)
            {
                logger.Info($"Выполнение: {plugin.Name}");

                await plugin.ExecuteAsync(logger, CancellationToken.None);

                logger.SaveToFile();
            }
        }

        private static async Task ExecutePlugin(INetFixPlugin plugin)
        {
            var logger = new CombinedLogHandler(new PrettyConsoleLogHandler(), new FileLogHandler());

            logger.Info($"Выполнение: {plugin.Name}");

            await plugin.ExecuteAsync(logger, CancellationToken.None);

            logger.SaveToFile();
        }
    }
}
