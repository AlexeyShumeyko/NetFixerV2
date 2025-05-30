using NetFixer.Interfaces;
using NetFixer.Utils;

namespace NetFixer.Plugins.Network
{
    public class TracertPlugin : INetFixPlugin
    {
        public string Name => "Проверка маршрута (tracert)";

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            var targets = new Dictionary<string, string>
            {
                { "fabrika-fotoknigi.com", "31.130.202.41" },
                { "online.fabrika-fotoknigi.com", "178.172.173.90" }
            };

            foreach (var pair in targets)
            {
                string domain = pair.Key;
                string ip = pair.Value;

                log.Info($"Трассировка маршрута до {domain}:");
                var result = await CommandExecutor.ExecuteAsync($"tracert -d {domain}", log);

                if (result.Output.Contains("Превышен интервал ожидания") || result.Output.Contains("100% потерь"))
                    log.Error($"Проблемы с доступом к {pair} — потеря пакетов.");

                if (IsDnsError(result.Output.ToString()))
                {
                    result = await CommandExecutor.ExecuteAsync($"tracert -d {ip}", log);
                    log.Info($"Трассировка маршрута по IP {ip} (fallback):\n{result}");
                }
            }
        }

        private bool IsDnsError(string output)
        {
            return output.Contains("не удалось разрешить") ||
                   output.Contains("could not find host", StringComparison.OrdinalIgnoreCase);
        }
    }
}
