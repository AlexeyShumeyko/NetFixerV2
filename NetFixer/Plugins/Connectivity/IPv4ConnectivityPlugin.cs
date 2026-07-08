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
                        using var tcp = new TcpClient(AddressFamily.InterNetwork);

                        var connectTask = tcp.ConnectAsync(ip, 443);

                        var timeoutTask =
                            Task.Delay(5000, token);

                        var completed =
                            await Task.WhenAny(connectTask, timeoutTask);

                        if (completed == timeoutTask)
                        {
                            log.Warning($"IPv4 {ip} : TIMEOUT");
                            continue;
                        }

                        if (tcp.Connected)
                        {
                            log.Success($"IPv4 {ip} : OK");
                        }
                        else
                        {
                            log.Error($"IPv4 {ip} : FAILED");
                        }
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
