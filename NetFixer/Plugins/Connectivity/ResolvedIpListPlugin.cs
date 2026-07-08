using NetFixer.Core;
using NetFixer.Interfaces;
using NetFixer.Resources;

namespace NetFixer.Plugins.Connectivity
{
    public class ResolvedIpListPlugin : INetFixPlugin
    {
        public string Name => "DNS результат";

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

                foreach (var address in addresses)
                {
                    log.Info(
                        address.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
