using NetFixer.Interfaces;
using System.Net.NetworkInformation;

namespace NetFixer.Plugins.Dns
{
    public class DnsServersPlugin : INetFixPlugin
    {
        public string Name => "DNS серверы";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    var dns =
                        nic.GetIPProperties()
                            .DnsAddresses;

                    foreach (var server in dns)
                    {
                        log.Info(
                            $"{nic.Name} -> {server}");
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
