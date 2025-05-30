using NetFixer.Interfaces;
using NetFixer.Utils;
using System.Text.RegularExpressions;

namespace NetFixer.Plugins.Network
{
    public class PingPlugin : INetFixPlugin
    {
        public string Name => "Проверка пинга до сервисов";

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            var targets = new Dictionary<string, string>
            {
                { "fabrika-fotoknigi.com", "31.130.202.41" },
                { "online.fabrika-fotoknigi.com", "178.172.173.90" }
            };

            foreach (var target in targets)
            {
                await TestConnection(log, target.Key, target.Value);
            }
        }

        private async Task TestConnection(ILog log, string domain, string ip)
        {
            log.Info($"Проверка пинга: {domain}");
            var result = await CommandExecutor.ExecuteAsync($"ping -n 4 {domain}", log);

            if (IsSuccessfulPing(result.Output))
            {
                log.Success($"{domain} отвечает нормально");

                return;
            }

            if (IsDnsError(result.Output))
            {
                log.Error($"Не удалось выполнить разрешение DNS, пробуем по IP {ip}...");
                result = await CommandExecutor.ExecuteAsync($"ping -n 4 {ip}", log);

                if (IsSuccessfulPing(result.Output))
                    log.Success($"{ip} отвечает нормально");
                else
                    log.Error($"Потеря пакета до {ip}");
            }
            else if (HasPacketLoss(result.Output))
            {
                log.Error($"Потеря пакета до {domain}");
            }
        }

        private bool IsSuccessfulPing(string output)
        {
            return Regex.IsMatch(output, @"Reply from.+time(?:=|<\d+ms)", RegexOptions.IgnoreCase) ||
                   Regex.IsMatch(output, @"Ответ от.+врем(?:я|ени)(?:=|<\d+мс)", RegexOptions.IgnoreCase);
        }

        private bool HasPacketLoss(string output)
        {
            return output.Contains("100% loss") ||
                   output.Contains("100% потерь") ||
                   output.Contains("Request timed out") ||
                   output.Contains("Превышен интервал ожидания");
        }

        private bool IsDnsError(string output)
        {
            return output.Contains("не удалось разрешить") ||
                   output.Contains("could not find host", StringComparison.OrdinalIgnoreCase);
        }
    }
}
