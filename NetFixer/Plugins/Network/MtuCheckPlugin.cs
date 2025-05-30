using NetFixer.Interfaces;
using NetFixer.Utils;
using System.Text.RegularExpressions;

namespace NetFixer.Plugins.Network
{
    public class MtuCheckPlugin : INetFixPlugin
    {
        public string Name => "Автофикс MTU";

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            int[] testSizes = { 1472, 1464, 1452, 1400, 1300 };
            int optimalMtu = -1;
            string testHost = "fabrika-fotoknigi.com";

            log.Info($"Поиск оптимального MTU по {testHost}...");

            foreach (int size in testSizes)
            {
                var pingResult = await CommandExecutor.ExecuteAsync(
                    $"ping -n 2 -f -l {size} {testHost}", log);

                if (!ContainsFragmentationError(pingResult.Output))
                {
                    optimalMtu = size + 28;

                    log.Success($"Оптимальный MTU: {optimalMtu} (размер пакета {size})");

                    break;
                }
            }

            if (optimalMtu == -1)
            {
                log.Error("MTU не определён. Пакеты фрагментируются даже при размере 1300.");

                return;
            }

            await ApplyMtuToInterfaces(log, optimalMtu);

            await CommandExecutor.ExecuteAsync("netsh interface ipv4 show interfaces", log);

            log.Info("Работа завершена. Перезапустите сетевой адаптер или ПК для применения MTU.");
        }

        private bool ContainsFragmentationError(string output)
        {
            return output.Contains("fragmentation needed") ||
                   output.Contains("требуется фрагментация") ||
                   output.Contains("Packet needs to be fragmented");
        }

        private async Task ApplyMtuToInterfaces(ILog log, int mtu)
        {
            var listResult = await CommandExecutor.ExecuteAsync(
            "netsh interface ipv4 show interfaces", log);

            var interfaces = ParseNetworkInterfaces(listResult.Output);

            foreach (var iface in interfaces)
            {
                if (ShouldSkipInterface(iface)) continue;

                log.Info($"Установка MTU {mtu} для {iface.Name}...");
                var setResult = await CommandExecutor.ExecuteAsync(
                    $"netsh interface ipv4 set subinterface \"{iface.Name}\" mtu={mtu} store=persistent", log);

                if (setResult.ExitCode == 0)
                    log.Success($"MTU {mtu} успешно установлен на интерфейс: {iface.Name}");
                else
                    log.Error($"Ошибка при установке MTU на {iface.Name}: {setResult.Output}");
            }
        }

        private List<NetworkInterface> ParseNetworkInterfaces(string output)
        {
            var interfaces = new List<NetworkInterface>();
            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = Regex.Split(line.Trim(), @"\s{2,}");
                if (parts.Length < 5) continue;

                if (int.TryParse(parts[0], out _))
                {
                    var state = parts[3];
                    var isConnected = state.Equals("connected", StringComparison.OrdinalIgnoreCase) ||
                                      state.Equals("подключен", StringComparison.OrdinalIgnoreCase);

                    if (isConnected)
                    {
                        interfaces.Add(new NetworkInterface
                        {
                            Id = parts[0],
                            Name = parts[4],
                            State = state,
                            CurrentMtu = parts[2]
                        });
                    }
                }
            }

            return interfaces;
        }

        private bool ShouldSkipInterface(NetworkInterface iface)
        {
            return iface.Name.Contains("Loopback", StringComparison.OrdinalIgnoreCase) ||
                   iface.Name.Contains("WSL", StringComparison.OrdinalIgnoreCase) ||
                   iface.Name.Contains("Pseudo", StringComparison.OrdinalIgnoreCase);
        }

        private class NetworkInterface
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string State { get; set; }
            public string CurrentMtu { get; set; }
        }
    }
}
