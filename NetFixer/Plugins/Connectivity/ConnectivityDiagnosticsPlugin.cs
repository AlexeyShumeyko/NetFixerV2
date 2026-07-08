using NetFixer.Interfaces;

namespace NetFixer.Plugins.Connectivity
{
    public class ConnectivityDiagnosticsPlugin : INetFixPlugin
    {
        private readonly INetFixPlugin[] _plugins =
            [
                new ResolvedIpListPlugin(),

                new TcpPerIpPlugin(),

                new IPv4ConnectivityPlugin(),
                new IPv6ConnectivityPlugin(),

                new TlsHandshakePlugin(),
                new CertificateInfoPlugin(),
                new SniValidationPlugin(),

                new HttpHeadPlugin(),
                new HttpGetPlugin(),
                new RedirectChainPlugin(),

                new ResponseHeadersPlugin(),
                new HttpTimingPlugin(),
                new HttpVersionPlugin(),

                new CurlCheckPlugin(),
                new HostsFileCheckPlugin(),
                new SiteAvailabilityPlugin(),

                new DualStackAnalysisPlugin(),

                new CdnDetectionPlugin(),
                new WafDetectionPlugin(),
            ];

        public string Name => "Глубокая диагностика подключения";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.StartPluginGroup(Name);

            foreach (var plugin in _plugins)
            {
                await plugin.ExecuteAsync(log, token);
            }
        }
    }
}
