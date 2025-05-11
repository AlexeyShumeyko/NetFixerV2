using NetFixer.Interfaces;
using NetFixer.Utils;

namespace NetFixer.Plugins.Dns
{
    public class DnsCheckNslookupPlugin : INetFixPlugin
    {
        public string Name => "Проверка DNS (nslookup)";

        private readonly List<string> _domain = new()
        {
            "fabrika-fotoknigi.com",
            "online.fabrika-fotoknigi.com",
            "check.fabrika-fotoknigi.com",
            "layflat.info"
        };

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.Info("Запуск nslookup для доменов");

            foreach (var domain in _domain)
            {
                var result = await CommandExecutor.ExecuteAsync($"nslookup {domain}", log);
                if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Output))
                    log.Success($"Резолвинг успешен: {domain}");
                else
                    log.Error($"Ошибка резолвинга: {domain}");
            }
        }
    }
}
