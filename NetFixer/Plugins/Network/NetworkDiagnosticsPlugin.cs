using NetFixer.Interfaces;

namespace NetFixer.Plugins.Network
{
    public class NetworkDiagnosticsPlugin : INetFixPlugin
    {
        public string Name => "Диагностика и восстановление сети";

        public async Task ExecuteAsync(ILog log, CancellationToken cancellationToken)
        {
            await new PingPlugin().ExecuteAsync(log, cancellationToken);
            await new TracertPlugin().ExecuteAsync(log, cancellationToken);
            await new MtuCheckPlugin().ExecuteAsync(log, cancellationToken);
            await new WinsockResetPlugin().ExecuteAsync(log, cancellationToken);
            await new ArpCacheClearPlugin().ExecuteAsync(log, cancellationToken);

            log.Info("Диагностика сети завершена");
        }
    }
}
