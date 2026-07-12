using NetFixer.Interfaces;
using NetFixer.Utils;

namespace NetFixer.Plugins.Dns
{
    public class DnsFlushPlugin : INetFixPlugin
    {
        public string Name => "Очистка DNS-кеша";

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.StartPluginGroup(Name);

            await Task.Delay(500);

            var result = await CommandExecutor.ExecuteAsync("ipconfig /flushdns", log, token);

            if (result.IsSuccess)
                log.Success("DNS-кеш успешно очищен");
            else
                log.Error($"Ошибка при очистке DNS-кеша: {result.Error}");
        }
    }
}
