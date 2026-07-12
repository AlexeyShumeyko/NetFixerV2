using NetFixer.Interfaces;
using NetFixer.Resources;
using System.Net.Sockets;

namespace NetFixer.Plugins.Connectivity
{
    public class IPv4ConnectivityPlugin : INetFixPlugin
    {
        public string Name => "IPv4 диагностика";

        public async Task ExecuteAsync(
        ILog log,
        CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                var addresses =
                    await System.Net.Dns.GetHostAddressesAsync(Targets.Site);

                var ipv4Addresses = addresses
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                    .ToArray();

                if (ipv4Addresses.Length == 0)
                {
                    log.Warning("IPv4 адреса не найдены.");
                    return;
                }

                foreach (var ip in ipv4Addresses)
                {
                    try
                    {
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                        cts.CancelAfter(5000);
                        using var tcp = new TcpClient(AddressFamily.InterNetwork);

                        await tcp.ConnectAsync(ip, 443).WaitAsync(cts.Token);
                        log.Success($"IPv4 {ip} : OK");
                    }
                    catch (OperationCanceledException)
                    {
                        log.Warning($"IPv4 {ip} : TIMEOUT");
                    }
                    catch (Exception ex)
                    {
                        log.Error($"IPv4 {ip} : {ex.Message}");
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