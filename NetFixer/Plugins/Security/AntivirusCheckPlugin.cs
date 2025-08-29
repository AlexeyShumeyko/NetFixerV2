using NetFixer.Interfaces;
using System.Diagnostics;
using System.ServiceProcess;

namespace NetFixer.Plugins.Security
{
    public class AntivirusCheckPlugin : INetFixPlugin
    {
        public string Name => "Проверка наличия антивирусного ПО";

        private readonly IReadOnlyList<string> _antivirusNames = new[]
        {
            "Kaspersky", "ESET", "DrWeb", "Avast", "BitDefender",
            "Norton", "McAfee", "Windows Defender", "360", "Avira"
        };

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.StartPluginGroup(Name);

            try
            {
                var detectedAntiviruses = await DetectAntiviruses(token);
                ReportDetectionResults(log, detectedAntiviruses);
            }
            catch (OperationCanceledException)
            {
                log.Info("Проверка антивирусов отменена");
                throw;
            }
        }

        private async Task<List<string>> DetectAntiviruses(CancellationToken token)
        {
            var detected = new List<string>();

            foreach (var avName in _antivirusNames)
            {
                if (await IsAntivirusInstalledAsync(avName))
                {
                    detected.Add(avName);
                }

                await Task.Yield();
            }

            return detected;
        }

        private async Task<bool> IsAntivirusInstalledAsync(string name)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var processes = Process.GetProcesses()
                        .Any(p => p.ProcessName.Contains(name, StringComparison.OrdinalIgnoreCase));

                    var services = ServiceController.GetServices()
                        .Any(s => s.DisplayName.Contains(name, StringComparison.OrdinalIgnoreCase));

                    return processes || services;
                }
                catch
                {
                    return false;
                }
            });
        }

        private void ReportDetectionResults(ILog log, List<string> detectedAntiviruses)
        {
            if (detectedAntiviruses.Count > 0)
            {
                foreach (var av in detectedAntiviruses)
                {
                    log.Error($"Обнаружен антивирус: {av}");
                }
                log.Info("Проверьте журнал блокировок в настройках антивируса");
            }
            else
            {
                log.Success("Антивирусы не обнаружены");
            }
        }
    }
}
