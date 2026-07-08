using NetFixer.Interfaces;
using NetFixer.Resources;

namespace NetFixer.Plugins.Connectivity
{
    public class HttpVersionPlugin : INetFixPlugin
    {
        public string Name => "HTTP версия";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                using var client =
                    HttpClientFactory.Create();

                var response =
                    await client.GetAsync(
                        Targets.HttpsUrl,
                        token);

                log.Info(
                    $"HTTP/{response.Version}");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
