using NetFixer.Core;
using NetFixer.Interfaces;
using NetFixer.Resources;

namespace NetFixer.Plugins.Dns
{
    public class SystemDnsResolvePlugin : INetFixPlugin
    {
        public string Name => "System DNS Resolve";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                var addresses =
                    DiagnosticContext
                    .Instance
                    .ResolvedAddresses;

                foreach (var ip in addresses)
                {
                    log.Info(ip.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
