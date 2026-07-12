using NetFixer.Interfaces;
using NetFixer.Utils;

namespace NetFixer.Plugins.Connectivity
{
    public class CurlCheckPlugin : INetFixPlugin
    {
        public string Name => "Проверка HTTP-соединения (curl)";

        private readonly string[] _testUrls = new[]
        {
            "https://fabrika-fotoknigi.com",
            "https://online.fabrika-fotoknigi.com"
        };

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.StartPluginGroup(Name);

            foreach (var url in _testUrls)
            {
                log.SubSection($"Проверка для {url}");

                await TestCurl(log, url, token);
            }
        }

        private async Task TestCurl(ILog log, string url, CancellationToken token)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(15000);

            try
            {
                var result = await CommandExecutor.ExecuteAsync(
                    $"curl -I --connect-timeout 10 {url}",
                    log,
                    logOutput: true,
                    logError: false,
                    cts.Token);

                if (result.IsSuccess && result.Output.Contains("HTTP/"))
                {
                    log.Success($"URL {url} доступен");

                    if (result.Output.Contains("200 OK"))
                        log.Info("Сервер отвечает нормально (200 OK)");
                }
                else
                {
                    log.Error($"URL {url} недоступен или превышено время ожидания");
                }
            }
            catch (OperationCanceledException)
            {
                log.Error($"URL {url}: curl timeout.");
            }
        }
    }
}