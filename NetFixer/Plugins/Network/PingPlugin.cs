using NetFixer.Core;
using NetFixer.Interfaces;
using NetFixer.Resources;
using NetFixer.Utils;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace NetFixer.Plugins.Network
{
    public class PingPlugin : INetFixPlugin
    {
        public string Name => "Ping диагностика";

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

                foreach (var ip in addresses)
                {
                    try
                    {
                        using var ping = new Ping();

                        var reply =
                            await ping.SendPingAsync(ip, 5000);

                        if (reply.Status == IPStatus.Success)
                        {
                            log.Success(
                                $"{ip} : {reply.RoundtripTime} ms");
                        }
                        else
                        {
                            log.Warning(
                                $"{ip} : {reply.Status}");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(
                            $"{ip}: {ex.Message}");
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
