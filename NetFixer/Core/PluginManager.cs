using NetFixer.Interfaces;
using NetFixer.Plugins.Dns;

namespace NetFixer.Core
{
    public static class PluginManager
    {
        public static List<INetFixPlugin> GetPlugins()
        {
            return new List<INetFixPlugin>
            {
                new DnsAutoOptimizePlugin()
            };
        }
    }
}
