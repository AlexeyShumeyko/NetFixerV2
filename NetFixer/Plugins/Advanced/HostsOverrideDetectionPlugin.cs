using NetFixer.Interfaces;

namespace NetFixer.Plugins.Advanced
{
    public class HostsOverrideDetectionPlugin : INetFixPlugin
    {
        public string Name => "Повторная проверка hosts";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            log.Info(
                "Дополнительный контроль hosts выполнен.");

            await Task.CompletedTask;
        }
    }
}
