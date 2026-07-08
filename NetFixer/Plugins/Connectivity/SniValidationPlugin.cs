using NetFixer.Interfaces;
using NetFixer.Resources;
using System.Net.Security;
using System.Net.Sockets;

namespace NetFixer.Plugins.Connectivity
{
    public class SniValidationPlugin : INetFixPlugin
    {
        public string Name => "SNI проверка";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                using var tcp = new TcpClient();

                await tcp.ConnectAsync(
                    Targets.Site,
                    443);

                using var ssl = new SslStream(
                    tcp.GetStream(),
                    false,
                    (_, _, _, _) => true);

                await ssl.AuthenticateAsClientAsync(
                    Targets.Site);

                log.Success(
                    $"SNI OK ({ssl.SslProtocol})");
            }
            catch (Exception ex)
            {
                log.Error(
                    $"SNI FAILED: {ex.Message}");
            }
        }
    }
}
