using NetFixer.Interfaces;
using NetFixer.Resources;
using NetFixer.Utils;

namespace NetFixer.Plugins.Network
{
    public class MtuDiscoveryPlugin : INetFixPlugin
    {
        public string Name => "MTU Discovery";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                var mtu =
                    await DetectOptimalMtu(
                        log);

                if (mtu > 0)
                {
                    log.Success(
                        $"Recommended MTU: {mtu}");
                }
                else
                {
                    log.Warning(
                        "Не удалось определить MTU.");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private async Task<int> DetectOptimalMtu(
            ILog log)
        {
            int low = 1200;
            int high = 1500;

            while (low <= high)
            {
                var mid =
                    (low + high) / 2;

                var payload =
                    mid - 28;

                var result =
                    await CommandExecutor.ExecuteAsync(
                        $"ping -f -n 1 -l {payload} {Targets.Site}",
                        log);

                if (IsSuccess(result.Output))
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return high;
        }

        private bool IsSuccess(
            string output)
        {
            return output.Contains(
                "TTL=",
                StringComparison.OrdinalIgnoreCase)
                ||
                output.Contains(
                "TTL:",
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
