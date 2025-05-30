using NetFixer.Interfaces;
using NetFixer.Utils;
using System.Text.RegularExpressions;

namespace NetFixer.Plugins.Connection
{
    public class DnsResetIpConfigPlugin : INetFixPlugin
    {
        public string Name => "Сброс/обновление IP-конфигурации";

        public async Task ExecuteAsync(ILog log, CancellationToken cancellationToken)
        {
            log.Info("Анализ сетевых интерфейсов: используется ли DHCP...");

            var result = await CommandExecutor.ExecuteAsync("netsh interface ip show config", log);

            if (!result.IsSuccess)
            {
                log.Error("Ошибка при получении конфигурации интерфейсов.");

                return;
            }

            var interfaces = ParseInterfaces(result.Output);
            var targetInterfaces = interfaces.Where(i => i.DhcpEnabled && i.HasGateway).ToList();

            if (!targetInterfaces.Any())
            {
                log.Info("Нет интерфейса с DHCP и основным шлюзом. Прерывание выполнения.");

                return;
            }

            log.Info($"Найдено подходящих интерфейсов: {string.Join(", ", targetInterfaces.Select(i => i.Name))}");

            foreach (var iface in targetInterfaces)
            {
                log.Info($"Сброс IP для интерфейса: {iface.Name}");

                await CommandExecutor.ExecuteAsync($"ipconfig /release \"{iface.Name}\"", log);
                await Task.Delay(3000);

                var renewResult = await CommandExecutor.ExecuteAsync($"ipconfig /renew \"{iface.Name}\"", log);

                var interfacePattern = new Regex($@"adapter {Regex.Escape(iface.Name)}:.*?\n(.*?)(?=adapter \w+:|$)",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);

                var interfaceMatch = interfacePattern.Match(renewResult.Output);
                if (!interfaceMatch.Success)
                {
                    log.Error($"Не удалось найти секцию интерфейса {iface.Name}");

                    return;
                }

                var ipPattern = new Regex(@"(?:IPv4 Address|IP[-\s]*адрес)[^:]*:\s*(\d+\.\d+\.\d+\.\d+)",
                    RegexOptions.IgnoreCase);
                var ipMatch = ipPattern.Match(interfaceMatch.Value);

                if (ipMatch.Success)
                    log.Success($"IP для {iface.Name} успешно обновлён: {ipMatch.Groups[1].Value}");
                else
                    log.Error($"Не удалось определить IP в секции интерфейса {iface.Name}");
            }

            log.Success("Сброс IP-конфигурации для всех интерфейсов выполнен.");

        }

        private List<NetworkInterfaceInfo> ParseInterfaces(string output)
        {
            var interfaces = new List<NetworkInterfaceInfo>();
            var lines = output.Split('\n');

            NetworkInterfaceInfo? current = null;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();

                if (line.StartsWith("Configuration for interface") || 
                    line.StartsWith("Настройка интерфейса"))
                {
                    if (current != null)
                        interfaces.Add(current);

                    var name = Regex.Match(line, @"""(.+?)""").Groups[1].Value;
                    current = new NetworkInterfaceInfo { Name = name };
                }
                else if (current != null)
                {
                    if (line.StartsWith("DHCP enabled:") || 
                        line.StartsWith("DHCP включен:"))
                    {
                        current.DhcpEnabled = line.Contains("Yes") || line.Contains("Да");
                    }

                    if (line.StartsWith("Default Gateway:") || 
                        line.StartsWith("Основной шлюз:"))
                    {
                        current.HasGateway = !string.IsNullOrWhiteSpace(line.Split(':').LastOrDefault()?.Trim());
                    }
                }
            }

            if (current != null)
                interfaces.Add(current);

            return interfaces;
        }

        private class NetworkInterfaceInfo
        {
            public string Name { get; set; } = string.Empty;
            public bool DhcpEnabled { get; set; }
            public bool HasGateway { get; set; }
        }
    }
}
