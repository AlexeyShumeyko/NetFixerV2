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

                client.Timeout =
                    TimeSpan.FromSeconds(10);

                using var request =
                    new HttpRequestMessage(
                        HttpMethod.Head,
                        Targets.HttpsUrl);

                var response =
                    await client.SendAsync(
                        request,
                        token);

                log.Success(
                    $"HEAD {(int)response.StatusCode} {response.StatusCode}");
            }
            catch (Exception ex)
            {
                log.Error(
                    $"HEAD FAILED: {ex.Message}");
            }
        }
    }
}
