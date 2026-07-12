using NetFixer.Interfaces;
using NetFixer.Resources;

namespace NetFixer.Plugins.Connectivity
{
    public class WafDetectionPlugin : INetFixPlugin
    {
        public string Name => "WAF диагностика";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                using var client =
                    HttpClientFactory.Create();

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(5000);

                using var response = await client.GetAsync(Targets.HttpsUrl, cts.Token);

                foreach (var header in response.Headers)
                {
                    var value =
                        string.Join(" ", header.Value);

                    if (value.Contains(
                        "cloudflare",
                        StringComparison.OrdinalIgnoreCase) ||
                        value.Contains(
                        "sucuri",
                        StringComparison.OrdinalIgnoreCase) ||
                        value.Contains(
                        "imperva",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        log.Warning(
                            $"{header.Key}: {value}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                log.Error("WAF detection: timeout.");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
