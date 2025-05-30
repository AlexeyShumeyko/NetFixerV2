using NetFixer.Interfaces;
using NetFixer.Utils;
using System.Text.RegularExpressions;

namespace NetFixer.Plugins.Dns
{
    public static class DnsUtils
    {
        // Получить все активные сетевые интерфейсы
        public static async Task<List<string>> GetActiveInterfacesAsync(ILog log)
        {
            var result = await CommandExecutor.ExecuteAsync("netsh interface show interface", log);

            var lines = result.Output.Split('\n');

            var interfaces = lines
                .Skip(3)
                .Where(line => line.Contains("Подключен") || line.Contains("Connected"))
                .Select(line =>
                {
                    var parts = Regex.Split(line.Trim(), @"\s{2,}");
                    return parts.LastOrDefault();
                })
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            return interfaces;
        }

        // Получить текущие DNS сервера
        public static async Task<List<string>> GetCurrentDnsAsync(ILog log, bool onlyNonGoogleCloudflare = false)
        {
            var result = await CommandExecutor.ExecuteAsync("netsh interface ip show config", log);

            var dnsServers = new List<string>();

            foreach (var line in result.Output.Split('\n'))
            {
                if (line.Contains("DNS-") || line.Contains("DNS Servers"))
                {
                    var dnsLine = line.Split(':').LastOrDefault()?.Trim();

                    if (!string.IsNullOrWhiteSpace(dnsLine))
                    {
                        if (onlyNonGoogleCloudflare)
                        {
                            if (!dnsLine.Contains("8.8.8.8") && !dnsLine.Contains("1.1.1.1"))
                                dnsServers.Add(dnsLine);
                        }
                        else
                        {
                            dnsServers.Add(dnsLine);
                        }
                    }
                }
            }

            return dnsServers;
        }

        // Установить DNS на все активные интерфейсы
        public static async Task<bool> SetDnsAsync(string primary, string secondary, ILog log)
        {
            var interfaces = await GetActiveInterfacesAsync(log);

            if (!interfaces.Any())
            {
                log.Error("Нет активных интерфейсов для установки DNS.");
                return false;
            }

            bool allSuccess = true;

            foreach (var iface in interfaces)
            {
                log.Info($"Настройка DNS для интерфейса: {iface}");

                var result1 = await CommandExecutor.ExecuteAsync(
                    $@"netsh interface ip set dns name=""{iface}"" static {primary}", log);

                var result2 = await CommandExecutor.ExecuteAsync(
                    $@"netsh interface ip add dns name=""{iface}"" {secondary} index=2", log);

                if (!result1.IsSuccess || !result2.IsSuccess)
                {
                    log.Error($"Ошибка при установке DNS для интерфейса: {iface}");
                    allSuccess = false;
                }
            }

            return allSuccess;
        }
    }
}
