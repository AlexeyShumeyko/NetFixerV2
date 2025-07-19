using NetFixer.Interfaces;

namespace NetFixer.Plugins.Connection
{
    public class ConnectionCheckPlugin : INetFixPlugin
    {
        public string Name => "Проверка и диагностика подключения";

        public async Task ExecuteAsync(ILog log, CancellationToken cancellationToken)
        {
            await new ProxyCheckPlugin().ExecuteAsync(log, cancellationToken);
            await new SiteAvailabilityPlugin().ExecuteAsync(log, cancellationToken);
            await new HostsFileCheckPlugin().ExecuteAsync(log, cancellationToken);
            await new DnsResetIpConfigPlugin().ExecuteAsync(log, cancellationToken);

            log.Info("Проверка подключения завершена");
        }
    }
}
