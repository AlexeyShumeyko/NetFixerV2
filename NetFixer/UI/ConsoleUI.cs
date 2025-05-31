using NetFixer.Core;
using NetFixer.Logging;
using Spectre.Console;

namespace NetFixer.UI
{
    public static class ConsoleUI
    {
        public static async Task RunAutomatedDiagnostics()
        {
            var mainColor = Color.SteelBlue1;
            var disabledColor = Color.Grey;

            Console.Clear();
            RenderMainInterface.MainInterface();

            var logger = new CombinedLogHandler(new PrettyConsoleLogHandler(), new FileLogHandler());
            var plugins = PluginManager.GetPlugins();

            await AnsiConsole.Progress()
                .Columns(
                [
                    new TaskDescriptionColumn(),
                    new ConditionalProgressBarColumn(),
                    new ConditionalPercentageColumn(),
                    new SpinnerColumn()
                ])
                .StartAsync(async ctx =>
                {
                    var dummy = ctx.AddTask("[black on black]HIDDEN_TASK[/]", autoStart: true, maxValue: 1);
                    dummy.StopTask();

                    var mainTask = ctx.AddTask("[grey]Выполнение диагностики[/]", maxValue: plugins.Count);

                    foreach (var plugin in plugins)
                    {
                        var pluginTask = ctx.AddTask($"[grey]{plugin.Name}[/]", maxValue: 1);

                        try
                        {
                            logger.Info($"Запуск: {plugin.Name}");

                            var run = Task.Run(async () =>
                            {
                                await plugin.ExecuteAsync(logger, CancellationToken.None);
                            });

                            while (!run.IsCompleted)
                            {
                                await Task.Delay(200);
                                pluginTask.Increment(0.01);
                            }

                            await run;

                            pluginTask.Value = 1;
                            pluginTask.Description($"[green]{plugin.Name}[/]");
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Ошибка в плагине {plugin.Name}: {ex.Message}");
                            pluginTask.Description($"[red]{plugin.Name}[/]");
                        }
                        finally
                        {
                            pluginTask.StopTask();
                            mainTask.Increment(1);
                            logger.SaveToFile();
                        }
                    }

                    mainTask.Description("[green]Выполнение диагностики[/]");
                });

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[green]Диагностика завершена[/]") { Style = Style.Parse("green dim") });
            AnsiConsole.MarkupLine("\n[grey]Результаты сохранены в лог-файл[/]");
            AnsiConsole.MarkupLine("[grey]Нажмите любую клавишу для выхода...[/]");
            Console.ReadKey(true);
        }
    }
}
