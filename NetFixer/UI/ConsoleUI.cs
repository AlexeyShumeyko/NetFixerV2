using NetFixer.Core;
using NetFixer.Logging;
using NetFixer.Resources;
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

            await DiagnosticContext.Instance.InitializeAsync(
                Targets.Site);

            var logger = new PrettyConsoleLogHandler();

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
                            logger.Group($"Запуск: {plugin.Name}");

                            var run = Task.Run(async () =>
                            {
                                try
                                {
                                    using var cts =
                                        CancellationTokenSource.CreateLinkedTokenSource(
                                            CancellationToken.None);

                                    cts.CancelAfter(TimeSpan.FromSeconds(10));

                                    await plugin.ExecuteAsync(
                                        logger,
                                        cts.Token);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(
                                        $"Plugin crash: {plugin.Name} -> {ex.Message}");
                                }
                            });

                            while (!run.IsCompleted)
                            {
                                await Task.Delay(100);

                                if (pluginTask.Value < 0.95)
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

        public static void ShowWindows7Warning()
        {
            Console.Clear();
            RenderMainInterface.MainInterface();

            AnsiConsole.MarkupLine("\n[red bold]✗ Программа не поддерживает Windows 7[/]");
            AnsiConsole.MarkupLine("[yellow]→ Пожалуйста, обновите систему до Windows 10+[/]");
            AnsiConsole.MarkupLine("[grey]→ Используйте другую операционную систему[/]");

            AnsiConsole.MarkupLine("\n[yellow]Нажмите любую клавишу для выхода...[/]");
            Console.ReadKey(true);
        }

        public static void ShowMacOsWarning()
        {
            Console.Clear();
            RenderMainInterface.MainInterface();

            AnsiConsole.MarkupLine("\n[red bold]✗ Программа не поддерживает macOS[/]");
            AnsiConsole.MarkupLine("[yellow]→ Используйте Windows 10 или новее[/]");
            AnsiConsole.MarkupLine("[grey]→ Программа разработана только для Windows[/]");

            AnsiConsole.MarkupLine("\n[yellow]Нажмите любую клавишу для выхода...[/]");
            Console.ReadKey(true);
        }
    }
}
