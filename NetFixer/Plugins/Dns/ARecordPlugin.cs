using NetFixer.Interfaces;
using NetFixer.Resources;
using NetFixer.Utils;

namespace NetFixer.Plugins.Dns
{
    public class ARecordPlugin : INetFixPlugin
    {
        public string Name => "A Record";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                await CommandExecutor.ExecuteAsync(
                    $"nslookup -type=A {Targets.Site}",
                    log, token);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
