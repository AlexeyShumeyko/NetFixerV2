using NetFixer.Interfaces;
using NetFixer.Utils;
using System.Text.RegularExpressions;

namespace NetFixer.Plugins.Dns
{
    public class DnsPingComparePlugin : INetFixPlugin
    {
        public string Name => "Сравнение пинга DNS";

        public async Task ExecuteAsync(ILog log, CancellationToken cancellationToken)
        {
            var best = await GetBestDnsAsync(log);
            log.Info($"Лучший DNS: {best}");
        }

        public async Task<string> GetBestDnsAsync(ILog log)
        {
            int googleAvg = await GetPingAverageAsync("8.8.8.8", log);
            int cloudflareAvg = await GetPingAverageAsync("1.1.1.1", log);

            if (googleAvg == int.MaxValue && cloudflareAvg == int.MaxValue)
            {
                log.Error("Не удалось получить ping ни для Google, ни для Cloudflare. Используем Google по умолчанию.");

                return "Google";
            }

            log.Info($"Google DNS ping avg: {(googleAvg == int.MaxValue ? "недоступен" : googleAvg + " ms")}");
            log.Info($"Cloudflare DNS ping avg: {(cloudflareAvg == int.MaxValue ? "недоступен" : cloudflareAvg + " ms")}");

            if (googleAvg < cloudflareAvg) return "Google";
            if (cloudflareAvg < googleAvg) return "Cloudflare";

            return "Google"; // По умолчанию
        }

        private async Task<int> GetPingAverageAsync(string ip, ILog log)
        {
            var result = await CommandExecutor.ExecuteAsync($"ping -n 3 {ip}", log);

            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Output))
            {
                log.Error($"Не удалось выполнить ping до {ip}");

                return int.MaxValue;
            }

            string output = result.Output;

            var matchEn = Regex.Match(output, @"Average\s*=\s*(\d+)", RegexOptions.IgnoreCase);
            if (matchEn.Success && int.TryParse(matchEn.Groups[1].Value, out int avgEn))
                return avgEn;

            var matchRu = Regex.Match(output, @"Средн[ее]*\s*=\s*(\d+)", RegexOptions.IgnoreCase);
            if (matchRu.Success && int.TryParse(matchRu.Groups[1].Value, out int avgRu))
                return avgRu;

            log.Error($"Не удалось извлечь среднее значение пинга из ответа ping до {ip}");

            return int.MaxValue;
        }
    }
}
