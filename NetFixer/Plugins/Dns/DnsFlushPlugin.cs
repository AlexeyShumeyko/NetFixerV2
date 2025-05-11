using NetFixer.Interfaces;
using NetFixer.Utils;
using System.Diagnostics;

namespace NetFixer.Plugins.Dns
{
    public class DnsFlushPlugin : INetFixPlugin
    {
        public string Name => "Очистка DNS-кеша";

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.Info("Запуск команды ipconfig /flushdns");

            await Task.Delay(500);

            var result = await CommandExecutor.ExecuteAsync("ipconfig /flushdns", log);

            if (result.IsSuccess)
                log.Success("DNS-кеш успешно очищен");
            else
                log.Error($"Ошибка при очистке DNS-кеша: {result.Error}");
        }
    }
}
