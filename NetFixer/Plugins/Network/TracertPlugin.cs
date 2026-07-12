using NetFixer.Interfaces;
using NetFixer.Resources;
using NetFixer.Utils;

namespace NetFixer.Plugins.Network
{
    public class TracertPlugin : INetFixPlugin
    {
        public string Name => "Трассировка маршрута";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                var result =
                    await CommandExecutor.ExecuteAsync(
                        $"tracert -d {Targets.Site}",
                        log, token);

                if (result.IsSuccess)
                {
                    log.Success(
                        "Трассировка выполнена.");
                }
                else
                {
                    log.Warning(
                        "Трассировка завершилась с ошибками.");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
