using NetFixer.Interfaces;

namespace NetFixer.Plugins.Network
{
    public class NetworkDiagnosticsPlugin : INetFixPlugin
    {
        private readonly INetFixPlugin[] _plugins =
            [
                new PingPlugin(),
                new TracertPlugin(),
                new WinsockResetPlugin(),
                new MtuDiscoveryPlugin()
            ];

        public string Name => "Сетевая диагностика";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.StartPluginGroup(Name);

            foreach (var plugin in _plugins)
            {
                await plugin.ExecuteAsync(
                    log,
                    token);
            }

            log.Info("Сетевая диагностика завершена.");
        }
    }
}
