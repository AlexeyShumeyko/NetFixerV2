using NetFixer.Interfaces;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NetFixer.Plugins.Environment
{
    public class LocalNetworkInfoPlugin : INetFixPlugin
    {
        public string Name => "Локальная сеть";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                var adapters =
                    NetworkInterface.GetAllNetworkInterfaces()
                        .Where(x =>
                            x.OperationalStatus == OperationalStatus.Up);

                foreach (var adapter in adapters)
                {
                    var props = adapter.GetIPProperties();

                    foreach (var address in props.UnicastAddresses)
                    {
                        if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                            continue;

                        log.Info(
                            $"{adapter.Name}: {address.Address}");
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
