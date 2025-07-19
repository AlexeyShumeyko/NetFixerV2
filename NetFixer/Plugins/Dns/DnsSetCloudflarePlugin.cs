using NetFixer.Interfaces;

namespace NetFixer.Plugins.Dns
{
    public class DnsSetCloudflarePlugin : INetFixPlugin
    {
        public string Name => "Установка Cloudflare DNS";

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.StartPluginGroup(Name);

            var success = await DnsUtils.SetDnsAsync("1.1.1.1", "1.0.0.1", log);

            if (success)
                log.Success("Cloudflare DNS успешно установлен");
            else
                log.Error("Не удалось установить Cloudflare DNS на все интерфейсы");
        }
    }
}
