using NetFixer.Interfaces;

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
            var nonCompliantDns = await new DnsCheckCurrentPlugin().GetCurrentDnsAsync(log);

            if (!nonCompliantDns.Any())
            {
                log.Info("Все интерфейсы уже используют Google или Cloudflare DNS. Повторная настройка не требуется.");

                return;
            }

            log.Info($"Найдено {nonCompliantDns.Count} интерфейсов с другими DNS: {string.Join(", ", nonCompliantDns)}");

            // Сравниваем Google и Cloudflare по ping
            var bestDns = await new DnsPingComparePlugin().GetBestDnsAsync(log);

            if (string.IsNullOrEmpty(bestDns))
            {
                log.Info("Оставляем текущие настройки.");
                return;
            }

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
