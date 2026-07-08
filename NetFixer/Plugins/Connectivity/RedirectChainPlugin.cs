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
                var handler =
                    new HttpClientHandler
                    {
                        AllowAutoRedirect = false
                    };

                using var client =
                    new HttpClient(handler);

                client.Timeout =
                    TimeSpan.FromSeconds(10);

                var currentUrl = Targets.HttpsUrl;

                for (var i = 0; i < 10; i++)
                {
                    var response =
                        await client.GetAsync(
                            currentUrl,
                            token);

                    log.Info(
                        $"{(int)response.StatusCode} -> {currentUrl}");

                    if (response.Headers.Location == null)
                        break;

                    currentUrl =
                        response.Headers.Location
                            .ToString();
                }

                log.Success(
                    "Redirect chain completed.");
            }
            catch (Exception ex)
            {
                log.Error(
                    ex.Message);
            }
        }
    }
}
