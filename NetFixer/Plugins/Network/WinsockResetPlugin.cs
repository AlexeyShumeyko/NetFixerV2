using NetFixer.Interfaces;
using NetFixer.Utils;

namespace NetFixer.Plugins.Network
{
    public class WinsockResetPlugin : INetFixPlugin
    {
        public string Name => "Сброс Winsock";

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.Info("Выполняем сброс Winsock.");

            var result = await CommandExecutor.ExecuteAsync("netsh winsock reset", log);

            if (result.Output.Contains("Successfully reset") ||
                result.Output.Contains("успешно сброшен") ||
                result.Output.Contains("Sucessfully reset"))
            {
                log.Success("Сброс выполнен. Рекомендуется перезапустить компьютер.");
            }
            else if (result.Output.Contains("Access is denied") ||
                     result.Output.Contains("Отказано в доступе"))
            {
                log.Error("Для сброса Winsock требуются права администратора");
            }
            else
                log.Error($"Сброс Winsock не удался: {result.Output.Trim()}");
        }
    }
}
