using NetFixer.Interfaces;
using NetFixer.Resources;

namespace NetFixer.Plugins.Connectivity
{
    public class WafDetectionPlugin : INetFixPlugin
    {
        public string Name => "WAF диагностика";

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

                foreach (var header in response.Headers)
                {
                    var value =
                        string.Join(" ", header.Value);

                    if (value.Contains(
                        "cloudflare",
                        StringComparison.OrdinalIgnoreCase) ||
                        value.Contains(
                        "sucuri",
                        StringComparison.OrdinalIgnoreCase) ||
                        value.Contains(
                        "imperva",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        log.Warning(
                            $"{header.Key}: {value}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
