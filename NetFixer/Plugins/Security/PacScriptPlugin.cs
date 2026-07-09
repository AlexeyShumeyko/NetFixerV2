using Microsoft.Win32;
using NetFixer.Interfaces;

namespace NetFixer.Plugins.Security
{
    public class PacScriptPlugin : INetFixPlugin
    {
        public string Name => "PAC Script";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                using var key =
                    Registry.CurrentUser.OpenSubKey(
                        @"Software\Microsoft\Windows\CurrentVersion\Internet Settings");

                var autoConfigUrl =
                    key?.GetValue("AutoConfigURL")
                        ?.ToString();

                if (string.IsNullOrWhiteSpace(
                    autoConfigUrl))
                {
                    log.Success(
                        "PAC Script не используется.");
                }
                else
                {
                    log.Warning(
                        $"PAC Script: {autoConfigUrl}");
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
