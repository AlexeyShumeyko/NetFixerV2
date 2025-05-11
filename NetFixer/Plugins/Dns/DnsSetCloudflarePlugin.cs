using NetFixer.Interfaces;
using NetFixer.Utils;

namespace NetFixer.Plugins.Dns
{
    public class DnsSetCloudflarePlugin : INetFixPlugin
    {
        public string Name => "Установить Cloudflare DNS";

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.Info("Установка Cloudflare DNS");

            var result1 = await CommandExecutor.ExecuteAsync(@"netsh interface ip set dns name=""Ethernet"" static 1.1.1.1", log);
            var result2 = await CommandExecutor.ExecuteAsync(@"netsh interface ip add dns name=""Ethernet"" 1.0.0.1 index=2", log);

            if (result1.IsSuccess && result2.IsSuccess)
                log.Success("Cloudflare DNS успешно установлен");
            else
                log.Error("Ошибка при установке Cloudflare DNS");
        }
    }
}
