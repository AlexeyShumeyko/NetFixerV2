using NetFixer.Interfaces;
using NetFixer.Utils;

namespace NetFixer.Plugins.Security
{
    public class FirewallCheckPlugin : INetFixPlugin
    {
        public string Name => "Проверка брандмауэра Windows (Firewall)";

        private readonly IReadOnlyDictionary<string, string> _targets = 
            new Dictionary<string, string>
        {
            { "fabrika-fotoknigi.com", "31.130.202.41" },
            { "online.fabrika-fotoknigi.com", "178.172.173.90" }
        };

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.StartPluginGroup(Name);

            try
            {
                var firewallRules = await GetFirewallRules(log);
                await CheckFirewallBlockages(log, firewallRules);
            }
            catch (OperationCanceledException)
            {
                log.Info("Проверка брандмауэра отменена");
            }
        }

        private async Task<string> GetFirewallRules(ILog log)
        {
            var result = await CommandExecutor.ExecuteAsync(
                "netsh advfirewall firewall show rule name=all",
                log,
                logOutput: false,
                logError: true);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException("Не удалось получить правила брандмауэра");
            }

            return result.Output;
        }

        private async Task CheckFirewallBlockages(ILog log, string firewallRules)
        {
            bool anyBlockageFound = false;

            foreach (var target in _targets)
            {
                if (IsFirewallBlocking(firewallRules, target.Key, target.Value))
                {
                    log.Error($"Обнаружена блокировка для {target.Key} [{target.Value}]");
                    anyBlockageFound = true;
                }
            }

            if (!anyBlockageFound)
                log.Success("Блокировок в брандмауэре не обнаружено");
        }

        private bool IsFirewallBlocking(string firewallOutput, string domain, string ip)
        {
            return firewallOutput.Contains(domain, StringComparison.OrdinalIgnoreCase) ||
                   firewallOutput.Contains(ip, StringComparison.OrdinalIgnoreCase);
        }
    }
}
