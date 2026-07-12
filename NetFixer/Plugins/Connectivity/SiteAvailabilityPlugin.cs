using NetFixer.Interfaces;
using NetFixer.Resources;
using System.Diagnostics;

namespace NetFixer.Plugins.Connectivity
{
    public class SiteAvailabilityPlugin : INetFixPlugin
    {
        public string Name => "Финальная проверка сайта";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                using var client = HttpClientFactory.Create();
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(10000);

                var stopwatch = Stopwatch.StartNew();
                using var response = await client.GetAsync(Targets.HttpsUrl, cts.Token);
                stopwatch.Stop();

                log.Success($"HTTP {(int)response.StatusCode} {response.StatusCode}");
                log.Info($"Response Time: {stopwatch.ElapsedMilliseconds} ms");

                if (response.Content.Headers.ContentLength is long length)
                {
                    log.Info($"Content Length: {length} bytes");
                }
            }
            catch (OperationCanceledException)
            {
                log.Error("Site check: Request timeout.");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}