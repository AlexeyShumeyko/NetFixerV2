using NetFixer.Interfaces;
using NetFixer.Resources;
using System.Net.Sockets;

namespace NetFixer.Plugins.Connectivity
{
    public class IPv6ConnectivityPlugin : INetFixPlugin
    {
        public string Name => "IPv6 диагностика";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                var addresses =
                    await System.Net.Dns.GetHostAddressesAsync(Targets.Site);

                var ipv6Addresses = addresses
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetworkV6)
                    .ToArray();

                if (ipv6Addresses.Length == 0)
                {
                    log.Info("IPv6 адреса отсутствуют.");
                    return;
                }

                foreach (var ip in ipv6Addresses)
                {
                    try
                    {
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                        cts.CancelAfter(5000);
                        using var tcp = new TcpClient(AddressFamily.InterNetworkV6);

                        await tcp.ConnectAsync(ip, 443).WaitAsync(cts.Token);
                        log.Success($"IPv6 {ip} : OK");
                    }
                    catch (OperationCanceledException)
                    {
                        log.Warning($"IPv6 {ip} : TIMEOUT");
                    }
                    catch (Exception ex)
                    {
                        log.Error($"IPv6 {ip} : {ex.Message}");
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