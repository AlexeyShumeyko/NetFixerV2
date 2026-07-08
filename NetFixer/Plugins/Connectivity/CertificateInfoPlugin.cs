using NetFixer.Interfaces;
using NetFixer.Resources;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace NetFixer.Plugins.Connectivity
{
    public class CertificateInfoPlugin : INetFixPlugin
    {
        public string Name => "Информация о сертификате";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                using var tcp =
                    new TcpClient();

                await tcp.ConnectAsync(
                    Targets.Site,
                    443);

                using var ssl =
                    new SslStream(
                        tcp.GetStream(),
                        false,
                        (_, _, _, _) => true);

                await ssl.AuthenticateAsClientAsync(
                    Targets.Site);

                var cert =
                    new X509Certificate2(
                        ssl.RemoteCertificate!);

                log.Info(
                    $"Subject: {cert.Subject}");

                log.Info(
                    $"Issuer: {cert.Issuer}");

                log.Info(
                    $"Not Before: {cert.NotBefore}");

                log.Info(
                    $"Not After: {cert.NotAfter}");

                log.Info(
                    $"Thumbprint: {cert.Thumbprint}");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
