using NetFixer.Interfaces;
using NetFixer.Plugins.Advanced;
using NetFixer.Plugins.Connectivity;
using NetFixer.Plugins.Dns;
using NetFixer.Plugins.Environment;
using NetFixer.Plugins.Network;
using NetFixer.Plugins.Routing;
using NetFixer.Plugins.Security;

namespace NetFixer.Core
{
    public static class PluginManager
    {
        private static readonly IReadOnlyList<INetFixPlugin> _plugins = new List<INetFixPlugin>
        {
            new ConnectivityDiagnosticsPlugin(),
            new RoutingDiagnosticsPlugin(),
            new DnsDiagnosticsPlugin(),
            new NetworkDiagnosticsPlugin(),
            new EnvironmentDiagnosticsPlugin(),
            new SecurityCheckPlugin(),
            new AdvancedDiagnosticsPlugin()
        };

        public static IReadOnlyList<INetFixPlugin> GetPlugins() => _plugins;

        public static async Task ExecuteAllAsync(ILog log, CancellationToken token)
        {
            foreach (var plugin in _plugins)
            {
                bool success = true;

                try
                {
                    log.StartPluginGroup(plugin.Name);

                    await plugin.ExecuteAsync(log, token);

                    log.Success($"Plugin finished: {plugin.Name}");
                }
                catch
                {
                    success = false;
                    log.Error($"Plugin failed: {plugin.Name}");
                    throw;
                }
                finally
                {
                    log.Group($"Plugin completed: {plugin.Name} | Success={success}");
                }
            }
        }
    }
}
