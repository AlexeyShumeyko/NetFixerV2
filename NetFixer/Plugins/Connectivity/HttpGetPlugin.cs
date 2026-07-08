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

                client.Timeout =
                    TimeSpan.FromSeconds(10);

                var response =
                    await client.GetAsync(
                        Targets.HttpsUrl,
                        token);

                log.Success(
                    $"GET {(int)response.StatusCode} {response.StatusCode}");
            }
            catch (Exception ex)
            {
                log.Error(
                    $"GET FAILED: {ex.Message}");
            }
        }
    }
}
