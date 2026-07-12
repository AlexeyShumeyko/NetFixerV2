using NetFixer.Interfaces;
using NetFixer.Resources;

namespace NetFixer.Plugins.Connectivity
{
    public class CdnDetectionPlugin : INetFixPlugin
    {
        public string Name => "CDN диагностика";

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
                    var key =
                        header.Key.ToLowerInvariant();

                    if (key.Contains("cf-") ||
                        key.Contains("cloudflare") ||
                        key.Contains("akamai") ||
                        key.Contains("fastly") ||
                        key.Contains("cdn"))
                    {
                        log.Info(
                            $"{header.Key}: {string.Join(", ", header.Value)}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                log.Error("CDN detection: timeout.");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
