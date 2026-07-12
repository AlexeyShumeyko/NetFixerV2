using NetFixer.Interfaces;
using NetFixer.Resources;

namespace NetFixer.Plugins.Connectivity
{
    public class HttpHeadPlugin : INetFixPlugin
    {
        public string Name => "HTTP HEAD";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                using var request = new HttpRequestMessage(HttpMethod.Head, Targets.HttpsUrl);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(10000);

                using var response = await client.SendAsync(request, cts.Token);

                log.Success($"HEAD {(int)response.StatusCode} {response.StatusCode}");
            }
            catch (OperationCanceledException)
            {
                log.Error("HEAD FAILED: Request timeout.");
            }
            catch (Exception ex)
            {
                log.Error($"HEAD FAILED: {ex.Message}");
            }
        }
    }
}