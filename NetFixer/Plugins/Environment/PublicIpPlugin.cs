using NetFixer.Interfaces;
using NetFixer.Resources;

namespace NetFixer.Plugins.Environment
{
    public class PublicIpPlugin : INetFixPlugin
    {
        public string Name => "Публичный IP";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                using var client = HttpClientFactory.Create();

                var ip =
                    await client.GetStringAsync(
                        "https://api.ipify.org",
                        token);

                log.Success(
                    $"Public IP: {ip}");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
