using NetFixer.Interfaces;

namespace NetFixer.Plugins.Network
{
    public class NetworkDiagnosticsPlugin : INetFixPlugin
    {
        public string Name => "Сеть: диагностика и восстановление";

        public async Task ExecuteAsync(ILog log, CancellationToken cancellationToken)
        {
            log.Info("Запущена диагностика сети");

            await new PingPlugin().ExecuteAsync(log, cancellationToken);
            await new TracertPlugin().ExecuteAsync(log, cancellationToken);
            await new MtuCheckPlugin().ExecuteAsync(log, cancellationToken);
            await new WinsockResetPlugin().ExecuteAsync(log, cancellationToken);

            log.Info("Диагностика сети завершена");
        }
    }
}
