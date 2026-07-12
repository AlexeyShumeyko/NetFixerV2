using NetFixer.Interfaces;
using NetFixer.Resources;

namespace NetFixer.Plugins.Connectivity
{
    public class HttpVersionPlugin : INetFixPlugin
    {
        public string Name => "HTTP версия";

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

                log.Info(
                    $"HTTP/{response.Version}");
            }
            catch (OperationCanceledException)
            {
                log.Error("HTTP version: timeout.");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
