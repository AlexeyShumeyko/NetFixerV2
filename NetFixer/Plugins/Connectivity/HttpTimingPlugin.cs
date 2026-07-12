using NetFixer.Interfaces;
using NetFixer.Resources;
using System.Diagnostics;

namespace NetFixer.Plugins.Connectivity
{
    public class HttpTimingPlugin : INetFixPlugin
    {
        public string Name => "HTTP время ответа";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                using var client =
                    HttpClientFactory.Create();

                var sw =
                    Stopwatch.StartNew();

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(5000);

                using var response = await client.GetAsync(Targets.HttpsUrl, cts.Token);

                sw.Stop();

                log.Info(
                    $"Status: {(int)response.StatusCode}");

                log.Info(
                    $"Response Time: {sw.ElapsedMilliseconds} ms");
            }
            catch (OperationCanceledException)
            {
                log.Error("HTTP timing: timeout.");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
