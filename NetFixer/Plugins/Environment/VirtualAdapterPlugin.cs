using NetFixer.Interfaces;
using System.Net.NetworkInformation;

namespace NetFixer.Plugins.Environment
{
    public class VirtualAdapterPlugin : INetFixPlugin
    {
        private static readonly string[] VirtualKeywords =
            [
                "virtual",
                "vmware",
                "hyper-v",
                "vbox",
                "virtualbox",
                "loopback"
            ];

        public string Name => "Виртуальные адаптеры";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                var found = false;

                foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
                {
                    var lower =
                        adapter.Description.ToLowerInvariant();

                    if (VirtualKeywords.Any(
                        x => lower.Contains(x)))
                    {
                        found = true;

                        log.Warning(
                            $"Обнаружен виртуальный адаптер: {adapter.Description}");
                    }
                }

                if (!found)
                {
                    log.Success(
                        "Виртуальные адаптеры не обнаружены.");
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
