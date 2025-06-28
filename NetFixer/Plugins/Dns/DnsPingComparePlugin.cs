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
            var googleResult = await GetPingResultAsync("8.8.8.8", log);
            var cloudflareResult = await GetPingResultAsync("1.1.1.1", log);

            if (googleResult.PacketLoss >= 100 && cloudflareResult.PacketLoss >= 100)
            {
                log.Error("Оба DNS недоступны (100% потерь). Оставляем текущие DNS без изменений.");
                return null;
            }

            log.Info($"Google DNS — Потери: {googleResult.PacketLoss}%, Пинг: {googleResult.AveragePing} мс");
            log.Info($"Cloudflare DNS — Потери: {cloudflareResult.PacketLoss}%, Пинг: {cloudflareResult.AveragePing} мс");

            if (googleResult.PacketLoss != cloudflareResult.PacketLoss)
                return googleResult.PacketLoss < cloudflareResult.PacketLoss ? "Google" : "Cloudflare";

            if (googleResult.AveragePing != cloudflareResult.AveragePing)
                return googleResult.AveragePing < cloudflareResult.AveragePing ? "Google" : "Cloudflare";

            return "Google";
        }

        private async Task<(int AveragePing, int PacketLoss)> GetPingResultAsync(string ip, ILog log)
        {
            var result = await CommandExecutor.ExecuteAsync($"ping -n 5 {ip}", log);

            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Output))
            {
                log.Error($"Не удалось выполнить ping до {ip}");

                return (int.MaxValue, 100);
            }

            var output = result.Output;

            // Потери пакетов
            int packetLoss = 100;
            var lossMatch = Regex.Match(output, @"Lost = (\d+)", RegexOptions.IgnoreCase);
            var sentMatch = Regex.Match(output, @"Sent = (\d+)", RegexOptions.IgnoreCase);

            if (lossMatch.Success && sentMatch.Success &&
                int.TryParse(lossMatch.Groups[1].Value, out int lost) &&
                int.TryParse(sentMatch.Groups[1].Value, out int sent) &&
                sent > 0)
            {
                packetLoss = (int)((double)lost / sent * 100);
            }

            // Средний пинг (EN и RU)
            int avgPing = int.MaxValue;
            var matchEn = Regex.Match(output, @"Average\s*=\s*(\d+)", RegexOptions.IgnoreCase);
            var matchRu = Regex.Match(output, @"Средн[ее]*\s*=\s*(\d+)", RegexOptions.IgnoreCase);

            if (matchEn.Success && int.TryParse(matchEn.Groups[1].Value, out int avgEn))
                avgPing = avgEn;
            else if (matchRu.Success && int.TryParse(matchRu.Groups[1].Value, out int avgRu))
                avgPing = avgRu;
            else
                log.Error($"Не удалось извлечь средний пинг из вывода ping для {ip}");

            return (avgPing, packetLoss);
        }
    }
}
