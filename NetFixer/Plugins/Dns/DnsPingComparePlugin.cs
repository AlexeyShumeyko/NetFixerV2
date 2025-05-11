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

            log.Info($"Google DNS ping avg: {googleAvg} ms");
            log.Info($"Cloudflare DNS ping avg: {cloudflareAvg} ms");

            if (googleAvg < cloudflareAvg) return "Google";
            if (cloudflareAvg < googleAvg) return "Cloudflare";

            return "Google"; // По умолчанию
        }

        private async Task<int> GetPingAverageAsync(string ip, ILog log)
        {
            var result = await CommandExecutor.ExecuteAsync($"ping -n 3 {ip}", log);

            var match = Regex.Match(result.Output, @"Среднее\s*=\s*(\d+)\s*мсек", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int avg))
                return avg;

            return int.MaxValue;
        }
    }
}
