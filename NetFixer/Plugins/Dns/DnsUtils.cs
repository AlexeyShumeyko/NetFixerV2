using NetFixer.Interfaces;
using NetFixer.Utils;

namespace NetFixer.Plugins.Dns
{
    public static class DnsUtils
    {
        // Получить все активные сетевые интерфейсы
        public static async Task<List<string>> GetActiveInterfacesAsync(ILog log)
        {
            var result = await CommandExecutor.ExecuteAsync("netsh interface show interface", log);

            var lines = result.Output.Split('\n');

            int headerIndex = Array.FindIndex(lines, line =>
                line.Contains("Admin State") &&
                line.Contains("State") &&
                line.Contains("Interface Name")
            );

            if (headerIndex == -1)
            {
                log.Error("Не удалось найти заголовок таблицы интерфейсов.");
                return new List<string>();
            }

            string headerLine = lines[headerIndex];
            int statePosition = headerLine.IndexOf("State");
            int namePosition = headerLine.IndexOf("Interface Name");

            if (namePosition == -1 || namePosition == -1)
            {
                log.Error("Не удалось определить позицию имени интерфейса.");
                return new List<string>();
            }

            var interfaces = new List<string>();

            for (int i = headerIndex + 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string state = line.Length > statePosition
                    ? line.Substring(statePosition, Math.Min(namePosition - statePosition, line.Length - statePosition)).Trim()
                    : "";

                if (state.Contains("Disconnected", StringComparison.OrdinalIgnoreCase) ||
                    state.Contains("Отключен", StringComparison.OrdinalIgnoreCase) ||
                    state.Contains("Down", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                bool isConnected = state.Contains("Connected", StringComparison.OrdinalIgnoreCase) ||
                                 state.Contains("Подключен", StringComparison.OrdinalIgnoreCase) ||
                                 state.Contains("Up", StringComparison.OrdinalIgnoreCase);

                if (!isConnected)
                    continue;

                string interfaceName = line.Length > namePosition
                    ? line.Substring(namePosition).Trim()
                    : null;

                if (!string.IsNullOrWhiteSpace(interfaceName))
                    interfaces.Add(interfaceName);
            }

            return interfaces;
        }

        // Получить текущие DNS сервера
        public static async Task<List<string>> GetCurrentDnsAsync(ILog log, bool onlyNonGoogleCloudflare = false)
        {
            var result = await CommandExecutor.ExecuteAsync("netsh interface ip show config", log);

            var dnsServers = new List<string>();
            var lines = result.Output.Split('\n').Select(line => line.Trim()).ToList();
            bool isDnsBlock = false;

            foreach (var line in lines)
            {
                if (line.StartsWith("Statically Configured DNS Servers") || line.StartsWith("DNS servers configured through DHCP"))
                {
                    var parts = line.Split(':');

                    if (parts.Length > 1)
                    {
                        var ip = parts[1].Trim();
                        if (!string.IsNullOrWhiteSpace(ip) && ip != "None")
                            dnsServers.Add(ip);
                    }
                    isDnsBlock = true;
                }

                else if (isDnsBlock)
                {
                    // Следующая строка после DNS может содержать второй адрес
                    if (string.IsNullOrWhiteSpace(line) || line.Contains(":"))
                    {
                        isDnsBlock = false;
                        continue;
                    }

                    if (line != "None")
                        dnsServers.Add(line.Trim());
                }
            }

            if (onlyNonGoogleCloudflare)
            {
                dnsServers = dnsServers
                    .Where(ip => ip != "8.8.8.8" && ip != "8.8.4.4" && ip != "1.1.1.1" && ip != "1.0.0.1")
                    .ToList();
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
