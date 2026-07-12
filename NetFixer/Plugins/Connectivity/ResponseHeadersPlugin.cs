using NetFixer.Interfaces;
using NetFixer.Resources;

namespace NetFixer.Plugins.Connectivity
{
    public class ResponseHeadersPlugin : INetFixPlugin
    {
        public string Name => "HTTP заголовки";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                using var client =
                    HttpClientFactory.Create();

                using var request =
                    new HttpRequestMessage(
                        HttpMethod.Head,
                        Targets.HttpsUrl);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(5000);

                using var response = await client.SendAsync(request, cts.Token);

                foreach (var header in response.Headers)
                {
                    log.Info(
                        $"{header.Key}: {string.Join(", ", header.Value)}");
                }
            }
            catch (OperationCanceledException)
            {
                log.Error("Response headers: timeout.");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
