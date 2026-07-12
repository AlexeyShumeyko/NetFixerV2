using NetFixer.Interfaces;
using NetFixer.Resources;

namespace NetFixer.Plugins.Connectivity
{
    public class HttpGetPlugin : INetFixPlugin
    {
        public string Name => "HTTP GET";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(10000);

                using var response = await client.GetAsync(Targets.HttpsUrl, cts.Token);

                log.Success($"GET {(int)response.StatusCode} {response.StatusCode}");
            }
            catch (OperationCanceledException)
            {
                log.Error("GET FAILED: Request timeout.");
            }
            catch (Exception ex)
            {
                log.Error($"GET FAILED: {ex.Message}");
            }
        }
    }
}