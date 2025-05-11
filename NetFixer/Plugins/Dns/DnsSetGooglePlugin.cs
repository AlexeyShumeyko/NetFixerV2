using NetFixer.Interfaces;
using NetFixer.Utils;

namespace NetFixer.Plugins.Dns
{
    public class DnsSetGooglePlugin : INetFixPlugin
    {
        public string Name => "Установить Google DNS";

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.Info("Установка Google DNS");

            var result1 = await CommandExecutor.ExecuteAsync(@"netsh interface ip set dns name=""Ethernet"" static 8.8.8.8", log);
            var result2 = await CommandExecutor.ExecuteAsync(@"netsh interface ip add dns name=""Ethernet"" 8.8.4.4 index=2", log);

            if (result1.IsSuccess && result2.IsSuccess)
                log.Success("Google DNS успешно установлен");
            else
                log.Error("Ошибка при установке Google DNS");
        }
    }
}
