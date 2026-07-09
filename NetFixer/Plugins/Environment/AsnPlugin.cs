using NetFixer.Interfaces;
using NetFixer.Resources;
using System.Text.Json;

namespace NetFixer.Plugins.Environment
{
    public class AsnPlugin : INetFixPlugin
    {
        public string Name => "ASN";

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

                if (doc.RootElement.TryGetProperty(
                    "as",
                    out var asn))
                {
                    log.Success(
                        $"ASN: {asn.GetString()}");
                }
                else
                {
                    log.Warning(
                        "ASN не найден.");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
