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
                        using var tcp =
                            new TcpClient(AddressFamily.InterNetworkV6);

                        var connectTask =
                            tcp.ConnectAsync(ip, 443);

                        var timeoutTask =
                            Task.Delay(5000, token);

                        var completed =
                            await Task.WhenAny(connectTask, timeoutTask);

                        if (completed == timeoutTask)
                        {
                            log.Warning($"IPv6 {ip} : TIMEOUT");
                            continue;
                        }

                        if (tcp.Connected)
                        {
                            log.Success($"IPv6 {ip} : OK");
                        }
                        else
                        {
                            log.Error($"IPv6 {ip} : FAILED");
                        }
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
