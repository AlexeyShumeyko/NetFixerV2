using NetFixer.Interfaces;
using System.Net.Sockets;

namespace NetFixer.Plugins.Connection
{
    public class PortAvailabilityPlugin : INetFixPlugin
    {
        public string Name => "Проверка доступности порта 443 (HTTPS)";

        private readonly int _port = 443;

        private readonly string[] _testHosts = new[]
        {
            "fabrika-fotoknigi.com",
            "online.fabrika-fotoknigi.com"
        };

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.StartPluginGroup(Name);

            foreach (var hostWithPort in _testHosts)
            {
                await TestPortTcpClientAsync(log, hostWithPort, _port, token);
            }
        }

        private async Task TestPortTcpClientAsync(ILog log, string host, int port, CancellationToken token)
        {
            try
            {
                using var tcpClient = new TcpClient();
                var connectTask = tcpClient.ConnectAsync(host, port);

                var completedTask = await Task.WhenAny(
                    connectTask, 
                    Task.Delay(TimeSpan.FromSeconds(5), token));

                if (completedTask == connectTask && tcpClient.Connected)
                    log.Success($"Порт {port} доступен на {host}");
                else
                    log.Error($"Порт {port} недоступен на {host}");
            }
            catch (OperationCanceledException)
            {
                log.Info($"Проверка порта {port} для {host} отменена");
            }
            catch (Exception ex)
            {
                log.Error($"Ошибка проверки порта {port} для {host}: {ex.Message}");
            }
        }
    }
}
