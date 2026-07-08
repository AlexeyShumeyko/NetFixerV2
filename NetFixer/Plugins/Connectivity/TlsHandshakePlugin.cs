using NetFixer.Interfaces;
using NetFixer.Resources;
using System.Net.Security;
using System.Net.Sockets;

namespace NetFixer.Plugins.Connectivity
{
    public class TlsHandshakePlugin : INetFixPlugin
    {
        public string Name => "TLS Handshake";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                using var tcp = new TcpClient();

                var connectTask =
                    tcp.ConnectAsync(Targets.Site, 443);

                var timeoutTask =
                    Task.Delay(5000, token);

                var completed =
                    await Task.WhenAny(connectTask, timeoutTask);

                if (completed == timeoutTask)
                {
                    log.Error("TCP timeout.");
                    return;
                }

                using var ssl =
                    new SslStream(
                        tcp.GetStream(),
                        false,
                        (_, _, _, _) => true);

                await ssl.AuthenticateAsClientAsync(
                    Targets.Site);

                log.Success(
                    $"TLS OK | Protocol: {ssl.SslProtocol}");
            }
            catch (Exception ex)
            {
                log.Error(
                    $"TLS FAILED: {ex.Message}");
            }
        }
    }
}
