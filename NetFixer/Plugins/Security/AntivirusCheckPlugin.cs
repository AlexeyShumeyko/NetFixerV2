using NetFixer.Interfaces;
using System.Management;

namespace NetFixer.Plugins.Security
{
    public class AntivirusCheckPlugin : INetFixPlugin
    {
        public string Name => "Проверка антивирусов";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                var searcher = new ManagementObjectSearcher(
                    @"root\SecurityCenter2",
                    "SELECT * FROM AntivirusProduct");

                var found = false;

                foreach (ManagementObject obj in searcher.Get())
                {
                    found = true;

                    var displayName =
                        obj["displayName"]?.ToString();

                    log.Info(
                        $"Антивирус: {displayName}");
                }

                if (!found)
                {
                    log.Warning(
                        "Антивирусы не обнаружены.");
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
