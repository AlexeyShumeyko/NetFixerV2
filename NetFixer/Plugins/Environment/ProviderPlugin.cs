using NetFixer.Interfaces;
using NetFixer.Resources;
using System.Text.Json;

namespace NetFixer.Plugins.Environment
{
    public class ProviderPlugin : INetFixPlugin
    {
        public string Name => "Интернет-провайдер";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                using var client = HttpClientFactory.Create();

                var json =
                    await client.GetStringAsync(
                        "http://ip-api.com/json",
                        token);

                using var doc =
                    JsonDocument.Parse(json);

                var isp =
                    doc.RootElement
                        .GetProperty("isp")
                        .GetString();

                log.Success(
                    $"ISP: {isp}");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
