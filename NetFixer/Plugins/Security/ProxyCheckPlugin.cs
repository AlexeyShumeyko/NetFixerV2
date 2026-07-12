using NetFixer.Interfaces;
using NetFixer.Utils;

namespace NetFixer.Plugins.Security
{
    public class ProxyCheckPlugin : INetFixPlugin
    {
        public string Name => "Проверка прокси (WinHTTP)";

        public async Task ExecuteAsync(ILog log, CancellationToken cancellationToken)
        {
            log.StartPluginGroup("Проверка настроек WinHTTP-прокси...");

            var result = await CommandExecutor.ExecuteAsync("netsh winhttp show proxy", log, cancellationToken);
            var output = result.Output;

            if (output.Contains("Direct access", StringComparison.OrdinalIgnoreCase) ||
                output.Contains("Прямой доступ", StringComparison.OrdinalIgnoreCase))
            {
                log.Success("Прокси-сервер не используется (Direct access).");
            }
            else
            {
                log.Error("Обнаружен прокси-сервер! Подробности:");
                log.Info(output);
            }
        }
    }
}
