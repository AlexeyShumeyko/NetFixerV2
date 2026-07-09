using NetFixer.Interfaces;

namespace NetFixer.Plugins.Routing
{
    public class RoutingDiagnosticsPlugin : INetFixPlugin
    {
        private readonly INetFixPlugin[] _plugins =
            [
                new PerIpLatencyPlugin(),
                new PerIpTcpPlugin(),
                new PerIpTlsPlugin(),
                new PerIpHttpPlugin()
            ];

        public string Name => "Маршрутизация";

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
