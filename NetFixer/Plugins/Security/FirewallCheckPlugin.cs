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

                using var process =
                    Process.Start(psi);

                var output =
                    await process!.StandardOutput
                        .ReadToEndAsync(token);

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
