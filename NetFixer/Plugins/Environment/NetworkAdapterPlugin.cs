using NetFixer.Interfaces;
using System.Net.NetworkInformation;

namespace NetFixer.Plugins.Environment
{
    public class NetworkAdapterPlugin : INetFixPlugin
    {
        public string Name => "Сетевые адаптеры";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
                {
                    log.Info(
                        $"{adapter.Name} | " +
                        $"{adapter.NetworkInterfaceType} | " +
                        $"{adapter.OperationalStatus}");
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
