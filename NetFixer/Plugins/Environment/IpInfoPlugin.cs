using NetFixer.Interfaces;
using NetFixer.Resources;
using System.Text.Json;

namespace NetFixer.Plugins.Environment
{
    public class IpInfoPlugin : INetFixPlugin
    {
        public string Name => "IP-информация";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                using var client = HttpClientFactory.Create();
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(10000);

                var json = await client.GetStringAsync("http://ip-api.com/json", cts.Token);

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Public IP
                if (root.TryGetProperty("query", out var query))
                {
                    var ip = query.GetString();
                    log.Success($"Public IP: {ip}");
                }

                // Geo
                if (root.TryGetProperty("country", out var country))
                    log.Info($"Country: {country.GetString()}");

                if (root.TryGetProperty("city", out var city))
                    log.Info($"City: {city.GetString()}");

                // ISP
                if (root.TryGetProperty("isp", out var isp))
                    log.Success($"ISP: {isp.GetString()}");

                // ASN
                if (root.TryGetProperty("as", out var asn))
                    log.Success($"ASN: {asn.GetString()}");
            }
            catch (OperationCanceledException)
            {
                log.Error("IP-информация: timeout.");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}