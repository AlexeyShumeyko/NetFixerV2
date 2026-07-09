using NetFixer.Interfaces;

namespace NetFixer.Plugins.Dns
{
    public class DnsDiagnosticsPlugin : INetFixPlugin
    {
        private readonly INetFixPlugin[] _plugins =
            [
                new DnsServersPlugin(),
                new DnsFlushPlugin(),
                new DnsCheckNslookupPlugin(),
                new SystemDnsResolvePlugin(),
                new GoogleDnsResolvePlugin(),
                new CloudflareDnsResolvePlugin(),
                new ARecordPlugin(),
                new AAAARecordPlugin(),
            ];

        public string Name => "DNS диагностика";

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
