using NetFixer.Interfaces;

namespace NetFixer.Plugins.Advanced;

public class FinalSummaryPlugin : INetFixPlugin
{
    public string Name =>
        "Итоговый анализ";

    public Task ExecuteAsync(ILog log, CancellationToken token)
    {
        log.Group("Final summary analysis started");

        log.Info("Analyzing diagnostic results...");

        return Task.CompletedTask;
    }
}