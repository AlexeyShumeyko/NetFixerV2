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
                        using var tcp = new TcpClient();

                        var connectTask = tcp.ConnectAsync(ip, 443);

                        var timeoutTask = Task.Delay(5000, token);

                        var completed =
                            await Task.WhenAny(connectTask, timeoutTask);

                        if (completed == timeoutTask)
                        {
                            log.Warning(
                                $"{ip} : TCP 443 TIMEOUT");
                            continue;
                        }

                        if (tcp.Connected)
                        {
                            log.Success(
                                $"{ip} : TCP 443 OK");
                        }
                        else
                        {
                            log.Error(
                                $"{ip} : TCP 443 FAILED");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(
                            $"{ip} : {ex.Message}");
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
