using NetFixer.Interfaces;
using System.Diagnostics;

namespace NetFixer.Plugins.Security
{
    public class FirewallCheckPlugin : INetFixPlugin
    {
        public string Name => "Проверка брандмауэра Windows (Firewall)";

        public async Task ExecuteAsync(
        ILog log,
        CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "advfirewall show allprofiles",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);

                if (process == null) 
                {
                    log.Error("Не удалось запустить netsh");

                    return;
                }

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(5000);

                string output;

                try
                {
                    output = await process.StandardOutput.ReadToEndAsync(cts.Token);
                    await process.WaitForExitAsync(cts.Token);
                }
                catch (OperationCanceledException) 
                {
                    try { process.Kill(true); } catch { }
                    log.Error("Firewall check: timeout.");

                    return;
                }

                if (output.Contains("ON"))
                {
                    log.Info(
                        "Брандмауэр включён.");
                }
                else
                {
                    log.Warning(
                        "Брандмауэр отключён.");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
