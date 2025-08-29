using NetFixer.Interfaces;
using NetFixer.Utils;

namespace NetFixer.Plugins.Network
{
    public class ArpCacheClearPlugin : INetFixPlugin
    {
        public string Name => "Очистка ARP-кэша";

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.StartPluginGroup(Name);

            var result = await CommandExecutor.ExecuteAsync("arp -d *", log);

            if (result.IsSuccess)
                log.Success("ARP-кэш успешно очищен");
            else
                log.Error("Не удалось очистить ARP-кэш");
        }
    }
}
