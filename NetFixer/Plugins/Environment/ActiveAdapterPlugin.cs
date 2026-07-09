using NetFixer.Interfaces;
using System.Net.NetworkInformation;

namespace NetFixer.Plugins.Environment
{
    public class ActiveAdapterPlugin : INetFixPlugin
    {
        public string Name => "Активный адаптер";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                var active =
                    NetworkInterface.GetAllNetworkInterfaces()
                        .Where(x =>
                            x.OperationalStatus ==
                            OperationalStatus.Up)
                        .OrderByDescending(x =>
                            x.Speed)
                        .FirstOrDefault();

                if (active == null)
                {
                    log.Warning(
                        "Активный адаптер не найден.");
                }
                else
                {
                    log.Success(
                        $"{active.Name} ({active.NetworkInterfaceType})");
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
