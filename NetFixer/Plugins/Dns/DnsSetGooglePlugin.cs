using NetFixer.Interfaces;

namespace NetFixer.Plugins.Dns
{
    public class DnsSetGooglePlugin : INetFixPlugin
    {
        public string Name => "Установить Google DNS";

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.Info("Установка Google DNS");

            var success = await DnsUtils.SetDnsAsync("8.8.8.8", "8.8.4.4", log);

            if (success)
                log.Success("Google DNS успешно установлен");
            else
                log.Error("Не удалось установить Google DNS на все интерфейсы");
        }
    }
}
