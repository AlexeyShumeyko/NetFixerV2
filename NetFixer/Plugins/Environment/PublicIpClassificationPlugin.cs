using NetFixer.Interfaces;

namespace NetFixer.Plugins.Environment
{
    public class PublicIpClassificationPlugin : INetFixPlugin
    {
        public string Name => "Тип подключения";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                log.Info(
                    "Проверить ASN и провайдера выше.");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

            await Task.CompletedTask;
        }
    }
}
