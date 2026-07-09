using NetFixer.Core;
using NetFixer.Interfaces;
using System.Net.Sockets;

namespace NetFixer.Plugins.Routing
{
    public class PerIpTcpPlugin : INetFixPlugin
    {
        public string Name => "TCP по каждому IP";

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
                        using var client =
                            new TcpClient();

                        var connectTask =
                            client.ConnectAsync(
                                ip,
                                443);

                        var completed =
                            await Task.WhenAny(
                                connectTask,
                                Task.Delay(
                                    5000,
                                    token));

                        if (completed ==
                            connectTask)
                        {
                            log.Success(
                                $"{ip} -> TCP OK");
                        }
                        else
                        {
                            log.Warning(
                                $"{ip} -> TCP TIMEOUT");
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
