using NetFixer.Interfaces;

namespace NetFixer.Plugins.Advanced
{
    public class CipherSuitePlugin : INetFixPlugin
    {
        public string Name => "Cipher Suites";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                log.Info(
                    "Проверка набора шифров поддерживается ОС.");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

            await Task.CompletedTask;
        }
    }
}
