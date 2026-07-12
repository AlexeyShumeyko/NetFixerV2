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
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                        cts.CancelAfter(5000);
                        using var client = new TcpClient();

                        try
                        {
                            await client.ConnectAsync(ip, 443).WaitAsync(cts.Token);
                            log.Success($"{ip} -> TCP OK");
                        }
                        catch (OperationCanceledException)
                        {
                            log.Warning($"{ip} -> TCP TIMEOUT");
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
