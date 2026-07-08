using NetFixer.Adapters;
using NetFixer.Core;
using NetFixer.Logging;
using NetFixer.Rendering.V3;
using NetFixer.Resources;
using NetFixer.UI;
using NetFixer.Utils;
using Spectre.Console;

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

await RunV3Diagnostics();

async Task RunV3Diagnostics()
{
    await DiagnosticContext.Instance.InitializeAsync(Targets.Site);

    var consoleLog = new PrettyConsoleLogHandler();

    var v3Adapter = new V3SmartAdapter(consoleLog);

    var plugins = PluginManager.GetPlugins();

    await AnsiConsole.Progress()
        .Columns([
            new TaskDescriptionColumn(),
            new ConditionalProgressBarColumn(),
            new ConditionalPercentageColumn(),
            new SpinnerColumn()
        ])
        .StartAsync(async ctx =>
        {
            var mainTask = ctx.AddTask("[grey]Выполнение диагностики[/]", maxValue: plugins.Count);

            foreach (var plugin in plugins)
            {
                var pluginTask = ctx.AddTask($"[grey]{plugin.Name}[/]", maxValue: 1);

                try
                {
                    v3Adapter.StartPluginGroup(plugin.Name);

                    var run = Task.Run(async () =>
                    {
                        try
                        {
                            using var cts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None);
                            cts.CancelAfter(TimeSpan.FromSeconds(10));
                            await plugin.ExecuteAsync(v3Adapter, cts.Token);
                        }
                        catch (Exception ex)
                        {
                            v3Adapter.Error($"Plugin crash: {plugin.Name} -> {ex.Message}");
                        }
                    });

                    while (!run.IsCompleted)
                    {
                        await Task.Delay(100);
                        if (pluginTask.Value < 0.95) pluginTask.Increment(0.01);
                    }

                    await run;
                    pluginTask.Value = 1;
                    pluginTask.Description($"[green]{plugin.Name}[/]");
                }
                catch (Exception ex)
                {
                    v3Adapter.Error($"Ошибка в плагине {plugin.Name}: {ex.Message}");
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

    v3Adapter.FinalizeReport();
    var report = v3Adapter.Report;

    var html = V3HtmlRenderer.Render(report);
    var path = Path.Combine(AppContext.BaseDirectory, "report-v3.html");
    File.WriteAllText(path, html);

    AnsiConsole.WriteLine();
    AnsiConsole.Write(new Rule("[green]Диагностика завершена[/]") { Style = Style.Parse("green dim") });
    AnsiConsole.MarkupLine("\n[grey]Результаты сохранены в report-v3.html[/]");
    AnsiConsole.MarkupLine("[grey]Нажмите любую клавишу для выхода...[/]");
    Console.ReadKey(true);
}