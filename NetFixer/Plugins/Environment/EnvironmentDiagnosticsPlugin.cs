using NetFixer.Interfaces;

namespace NetFixer.Plugins.Environment
{
    public class EnvironmentDiagnosticsPlugin : INetFixPlugin
    {
        private readonly INetFixPlugin[] _plugins =
            [
                new IpInfoPlugin(),
                new LocalNetworkInfoPlugin(),
                new NetworkAdapterPlugin(),
                new VpnDetectionPlugin(),
                new VirtualAdapterPlugin(),
                new ActiveAdapterPlugin(),
                new DefaultGatewayPlugin(),
                new PublicIpClassificationPlugin()
            ];

        public string Name => "Диагностика окружения";

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
        }
    }
}
