using NetFixer.Interfaces;

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
            return await DnsUtils.GetCurrentDnsAsync(log, onlyNonGoogleCloudflare: true);
        }
    }
}
