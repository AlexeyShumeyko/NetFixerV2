using NetFixer.Interfaces;
using NetFixer.Resources;

namespace NetFixer.Plugins.Connectivity
{
    public class RedirectChainPlugin : INetFixPlugin
    {
        public string Name => "Redirect Chain";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = false
                };

                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(5);

                var currentUrl = Targets.HttpsUrl;

                for (var i = 0; i < 10; i++)
                {
                    token.ThrowIfCancellationRequested();

                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                    cts.CancelAfter(5000);

                    try
                    {
                        using var response = await client.GetAsync(currentUrl, cts.Token);

                        log.Info($"{(int)response.StatusCode} -> {currentUrl}");

                        if (response.Headers.Location == null)
                            break;

                        currentUrl = response.Headers.Location.ToString();
                    }
                    catch (OperationCanceledException)
                    {
                        log.Warning("Redirect chain: timeout.");
                        break;
                    }
                }

                log.Success("Redirect chain completed.");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
