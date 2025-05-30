using NetFixer.Interfaces;

namespace NetFixer.Plugins.Connection
{
    public class SiteAvailabilityPlugin : INetFixPlugin
    {
        public string Name => "Проверка доступности сайта (HTTP/HTTPS)";

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            var url = "https://fabrika-fotoknigi.com";

            log.Info("Проверка доступности сайта (HTTP/HTTPS)");

            try
            {
                var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                var response = await client.GetAsync(url, token);

                log.Info($"HTTP статус: {response.StatusCode}");
                log.Info($"Время ответа: {response.Headers.Date}");
            }
            catch (HttpRequestException ex)
            {
                log.Error($"Ошибка HTTP: {ex.StatusCode} - {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                log.Error("Таймаут подключения (возможно блокировка)");
            }
        }
    }
}
