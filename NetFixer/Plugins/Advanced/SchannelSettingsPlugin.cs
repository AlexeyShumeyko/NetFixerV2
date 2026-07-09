using Microsoft.Win32;
using NetFixer.Interfaces;

namespace NetFixer.Plugins.Advanced
{
    public class SchannelSettingsPlugin : INetFixPlugin
    {
        public string Name => "Schannel";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                using var key =
                    Registry.LocalMachine.OpenSubKey(
                        @"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL");

                if (key == null)
                {
                    log.Warning(
                        "Раздел Schannel не найден.");
                }
                else
                {
                    log.Success(
                        "Schannel найден.");
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
