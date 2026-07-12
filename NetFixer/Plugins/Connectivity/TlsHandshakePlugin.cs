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
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(5000);
                using var tcp = new TcpClient();

                await tcp.ConnectAsync(Targets.Site, 443).WaitAsync(cts.Token);

                using var ssl = new SslStream(
                    tcp.GetStream(),
                    false,
                    (_, _, _, _) => true);

                await ssl.AuthenticateAsClientAsync(Targets.Site).WaitAsync(cts.Token);

                log.Success($"TLS OK | Protocol: {ssl.SslProtocol}");
            }
            catch (OperationCanceledException)
            {
                log.Error("TCP timeout.");
            }
            catch (Exception ex)
            {
                log.Error($"TLS FAILED: {ex.Message}");
            }
        }
    }
}