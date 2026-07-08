using NetFixer.Core;
using NetFixer.Interfaces;
using NetFixer.Resources;
using System.Net.Sockets;

namespace NetFixer.Plugins.Connectivity
{
    public class DualStackAnalysisPlugin : INetFixPlugin
    {
        public string Name => "IPv4 / IPv6 анализ";

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

                var ipv4 =
                    addresses.Count(x =>
                        x.AddressFamily ==
                        AddressFamily.InterNetwork);

                var ipv6 =
                    addresses.Count(x =>
                        x.AddressFamily ==
                        AddressFamily.InterNetworkV6);

                log.Info(
                    $"IPv4: {ipv4}");

                log.Info(
                    $"IPv6: {ipv6}");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
