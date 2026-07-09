using NetFixer.Core;
using NetFixer.Interfaces;
using System.Net.NetworkInformation;

namespace NetFixer.Plugins.Routing
{
    public class PerIpLatencyPlugin : INetFixPlugin
    {
        public string Name => "Ping по каждому IP";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                var addresses =
                    DiagnosticContext
                    .Instance
                    .ResolvedAddresses;

                using var ping =
                    new Ping();

                foreach (var ip in addresses)
                {
                    try
                    {
                        var reply =
                            await ping.SendPingAsync(
                                ip,
                                3000);

                        if (reply.Status ==
                            IPStatus.Success)
                        {
                            log.Success(
                                $"{ip} -> {reply.RoundtripTime} ms");
                        }
                        else
                        {
                            log.Warning(
                                $"{ip} -> {reply.Status}");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(
                            $"{ip} -> {ex.Message}");
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
