using NetFixer.Interfaces;
using NetFixer.Utils;

namespace NetFixer.Plugins.Dns
{
    public class DnsCheckCurrentPlugin : INetFixPlugin
    {
        public string Name => "Показать текущие DNS";

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.Info("Получение текущих настроек DNS");

            var currentDns = await GetCurrentDnsAsync(log);

            if (currentDns.Count == 0)
                log.Error("DNS-сервера не найдены.");
            else
            {
                foreach (var line in currentDns)
                    log.Info(line);
            }
        }

        public async Task<List<string>> GetCurrentDnsAsync(ILog log)
        {
            log.Info("Получение текущих настроек DNS");

            var result = await CommandExecutor.ExecuteAsync("netsh interface ip show config", log);

            return result.Output
                .Split('\n')
                .Where(l => l.Contains("DNS-"))
                .Select(l => l.Trim().Split(':').Last().Trim())
                .Where(ip => !string.IsNullOrWhiteSpace(ip))
                .ToList();
        }
    }
}
