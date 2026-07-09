using NetFixer.Interfaces;
using NetFixer.Resources;
using System.Text.Json;

namespace NetFixer.Plugins.Environment
{
    public class GeoIpPlugin : INetFixPlugin
    {
        public string Name => "GeoIP";

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

                var root =
                    doc.RootElement;

                var country =
                    root.GetProperty("country")
                        .GetString();

                var city =
                    root.GetProperty("city")
                        .GetString();

                log.Info(
                    $"Country: {country}");

                log.Info(
                    $"City: {city}");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
