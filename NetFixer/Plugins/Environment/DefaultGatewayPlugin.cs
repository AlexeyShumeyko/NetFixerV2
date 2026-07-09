using NetFixer.Interfaces;
using System.Net.NetworkInformation;

namespace NetFixer.Plugins.Environment
{
    public class DefaultGatewayPlugin : INetFixPlugin
    {
        public string Name => "Шлюз по умолчанию";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    var gateways =
                        nic.GetIPProperties()
                            .GatewayAddresses;

                    foreach (var gateway in gateways)
                    {
                        log.Info(
                            $"{nic.Name} -> {gateway.Address}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

            await Task.CompletedTask;
        }
    }
}
