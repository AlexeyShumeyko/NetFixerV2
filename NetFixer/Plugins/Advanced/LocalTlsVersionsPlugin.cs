using NetFixer.Interfaces;

namespace NetFixer.Plugins.Advanced
{
    public class LocalTlsVersionsPlugin : INetFixPlugin
    {
        public string Name => "TLS версии Windows";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            log.Info(
                $"SecurityProtocol: {System.Net.ServicePointManager.SecurityProtocol}");

            await Task.CompletedTask;
        }
    }
}
