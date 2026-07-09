using NetFixer.Interfaces;

namespace NetFixer.Plugins.Advanced
{
    public class AdvancedDiagnosticsPlugin : INetFixPlugin
    {
        private readonly INetFixPlugin[] _plugins =
            [
                new LocalTlsVersionsPlugin(),
                new CipherSuitePlugin(),
                new SchannelSettingsPlugin(),
                new HostsOverrideDetectionPlugin(),
                new FinalSummaryPlugin()
            ];

        public string Name => "Расширенная диагностика";

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
