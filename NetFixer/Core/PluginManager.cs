using NetFixer.Interfaces;
using NetFixer.Plugins;
using NetFixer.Plugins.Connection;
using NetFixer.Plugins.Dns;
using NetFixer.Plugins.Network;
using NetFixer.Plugins.Security;

namespace NetFixer.Core
{
    public static class PluginManager
    {
        private static readonly IReadOnlyList<INetFixPlugin> _plugins = new List<INetFixPlugin>
        {
            new DnsAutoOptimizePlugin(),
            new NetworkDiagnosticsPlugin(),
            new SecurityCheckPlugin(),
            new ConnectionCheckPlugin()
        };

        public static IReadOnlyList<INetFixPlugin> GetPlugins() => _plugins;
    }
}
