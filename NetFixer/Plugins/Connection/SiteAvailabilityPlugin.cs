using NetFixer.Interfaces;
using System.Diagnostics;

namespace NetFixer.Plugins.Connection
{
    public class SiteAvailabilityPlugin : INetFixPlugin
    {
        public string Name => "Проверка доступности сайта (HTTP/HTTPS)";

        private readonly List<string> _targets = new() 
        {
            "https://fabrika-fotoknigi.com",
            "https://online.fabrika-fotoknigi.com"
        };

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.StartPluginGroup(Name);

            foreach (var url in _targets)
            {
                await TestSiteAvailability(log, url, token);
            }
        }

        private async Task TestSiteAvailability(ILog log, string url, CancellationToken token)
        {
            log.SubSection($"Проверка: {url}");

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                var stopwatch = Stopwatch.StartNew();

                var response = await client.GetAsync(url, token);
                stopwatch.Stop();

                var ttfb = stopwatch.ElapsedMilliseconds;
                var status = response.StatusCode;

                log.Info($"HTTP статус: {status}");
                log.Info($"Время ответа (TTFB) {ttfb} мс");
                log.Info($"Размер ответа: {response.Content.Headers.ContentLength ?? 0} байт");

                if (response.IsSuccessStatusCode)
                    log.Success($"Успешное подключение ({ttfb} мс)");
                else
                    log.Error($"Сервер ответил с кодом: {(int)status} {status}");
            }
            catch (TaskCanceledException ex)
            {
                log.Error("ERR_TIME_OUT: Превышено время ожидания");
                log.SubSection($"Exception: {ex}");
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("connection reset"))
            {
                log.Error($"ERR_CONNECTION_RESET: Соединение сброшено");
                log.SubSection($"Exception: {ex.StatusCode} - {ex.Message}");
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("SSL"))
            {
                log.Error("ERR_SSL_PROTOCOL_ERROR: Ошибка SSL/TLS");
                log.SubSection($"Exception: {ex.StatusCode} - {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                log.Error($"Ошибка HTTP: {ex.Message}");
            }
            catch (Exception ex)
            {
                log.Error($"Неизвестная ошибка: {ex.Message}");
            }
        }
    }
}
