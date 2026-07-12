using NetFixer.Interfaces;
using NetFixer.Resources;
using NetFixer.Utils;

namespace NetFixer.Plugins.Dns
{
    public class GoogleDnsResolvePlugin : INetFixPlugin
    {
        public string Name => "Google DNS Resolve";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                await CommandExecutor.ExecuteAsync(
                    $"nslookup {Targets.Site} 8.8.8.8",
                    log, token);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
