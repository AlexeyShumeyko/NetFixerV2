using NetFixer.Interfaces;

namespace NetFixer.Plugins.Security
{
    public class SecurityCheckPlugin : INetFixPlugin
    {
        public string Name => "Безопасность - диагностика систем";

        public async Task ExecuteAsync(ILog log, CancellationToken cancellationToken)
        {
            await new AntivirusCheckPlugin().ExecuteAsync(log, cancellationToken);
            await new FirewallCheckPlugin().ExecuteAsync(log, cancellationToken);
            await new ProxyCheckPlugin().ExecuteAsync(log, cancellationToken);
            await new PacScriptPlugin().ExecuteAsync(log, cancellationToken);

            log.Info("Проверка безопасности завершена");
        }
    }
}
