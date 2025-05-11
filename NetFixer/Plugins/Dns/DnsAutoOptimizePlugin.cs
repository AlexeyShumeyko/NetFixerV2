using NetFixer.Interfaces;
using NetFixer.Utils;
using System.Text.RegularExpressions;

namespace NetFixer.Plugins.Dns
{
    public class DnsAutoOptimizePlugin : INetFixPlugin
    {
        public string Name => "DNS: Автоматическая оптимизация";

        public async Task ExecuteAsync(ILog log, CancellationToken cancellationToken)
        {
            log.Info("Запущена автоматическая оптимизация DNS");

            //Очистка кеша DNS
            await new DnsFlushPlugin().ExecuteAsync(log, cancellationToken);

            //Проверка текущих DNS
            var currentDns = await new DnsCheckCurrentPlugin().GetCurrentDnsAsync(log);

            if (currentDns.Any(ip => ip.Contains("8.8.8.8") || ip.Contains("1.1.1.1")))
            {
                log.Info("Уже используется Google или Cloudflare DNS. Повторная настройка не требуется.");

                return;
            }

            // Сравниваем Google и Cloudflare по ping
            var bestDns = await new DnsPingComparePlugin().GetBestDnsAsync(log);

            INetFixPlugin dnsSetter = bestDns == "Google"
                ? new DnsSetGooglePlugin()
                : new DnsSetCloudflarePlugin();

            log.Info($"Выбран лучший DNS: {bestDns}. Установка...");
            await dnsSetter.ExecuteAsync(log, cancellationToken);

            await new DnsCheckNslookupPlugin().ExecuteAsync(log, cancellationToken);

            log.Info("Оптимизация DNS завершена успешно");
        }
    }
}
