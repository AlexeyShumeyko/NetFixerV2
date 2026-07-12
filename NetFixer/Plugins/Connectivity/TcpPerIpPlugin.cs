using NetFixer.Interfaces;
using NetFixer.Resources;
using System.Net.Sockets;

namespace NetFixer.Plugins.Connectivity
{
    public class TcpPerIpPlugin : INetFixPlugin
    {
        public string Name => "TCP проверка всех IP";

        public async Task ExecuteAsync(
        ILog log,
        CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                var addresses = await System.Net.Dns.GetHostAddressesAsync(Targets.Site);

                if (addresses.Length == 0)
                {
                    log.Error("DNS не вернул ни одного IP.");
                    return;
                }

                foreach (var ip in addresses)
                {
                    try
                    {
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                        cts.CancelAfter(5000);
                        using var tcp = new TcpClient();

                        await tcp.ConnectAsync(ip, 443).WaitAsync(cts.Token);
                        log.Success($"{ip} : TCP 443 OK");
                    }
                    catch (OperationCanceledException)
                    {
                        log.Warning($"{ip} : TCP 443 TIMEOUT");
                    }
                    catch (Exception ex)
                    {
                        log.Error($"{ip} : {ex.Message}");
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