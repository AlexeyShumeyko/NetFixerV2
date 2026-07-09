using NetFixer.Core;
using NetFixer.Interfaces;
using NetFixer.Resources;
using System.Net.Security;
using System.Net.Sockets;

namespace NetFixer.Plugins.Routing
{
    public class PerIpTlsPlugin : INetFixPlugin
    {
        public string Name => "TLS по каждому IP";

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
                        using var tcp =
                            new TcpClient();

                        await tcp.ConnectAsync(
                            ip,
                            443);

                        using var ssl =
                            new SslStream(
                                tcp.GetStream(),
                                false,
                                (_, _, _, _) => true);

                        await ssl.AuthenticateAsClientAsync(
                            Targets.Site);

                        log.Success(
                            $"{ip} -> TLS OK");
                    }
                    catch (Exception ex)
                    {
                        log.Warning(
                            $"{ip} -> TLS FAIL: {ex.Message}");
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
