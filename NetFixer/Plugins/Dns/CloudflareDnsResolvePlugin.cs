using NetFixer.Interfaces;
using NetFixer.Resources;
using NetFixer.Utils;

namespace NetFixer.Plugins.Dns
{
    public class CloudflareDnsResolvePlugin : INetFixPlugin
    {
        public string Name => "Cloudflare DNS Resolve";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                await CommandExecutor.ExecuteAsync(
                    $"nslookup {Targets.Site} 1.1.1.1",
                    log);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
