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

                var response =
                    await client.GetAsync(
                        Targets.HttpsUrl,
                        token);

                sw.Stop();

                log.Info(
                    $"Status: {(int)response.StatusCode}");

                log.Info(
                    $"Response Time: {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
