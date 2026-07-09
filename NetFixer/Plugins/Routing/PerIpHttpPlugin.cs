using NetFixer.Core;
using NetFixer.Interfaces;
using NetFixer.Resources;
using System.Net.Sockets;

namespace NetFixer.Plugins.Routing
{
    public class PerIpHttpPlugin : INetFixPlugin
    {
        public string Name => "HTTP по каждому IP";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                var addresses =
                    DiagnosticContext
                    .Instance
                    .ResolvedAddresses;

                foreach (var ip in addresses)
                {
                    try
                    {
                        var handler =
                            new SocketsHttpHandler
                            {
                                ConnectCallback =
                                    async (ctx, ct) =>
                                    {
                                        var socket =
                                            new Socket(
                                                ip.AddressFamily,
                                                SocketType.Stream,
                                                ProtocolType.Tcp);

                                        await socket.ConnectAsync(
                                            ip,
                                            443,
                                            ct);

                                        return new NetworkStream(
                                            socket,
                                            ownsSocket: true);
                                    }
                            };

                        using var client =
                            new HttpClient(
                                handler);

                        client.DefaultRequestHeaders.Host =
                            Targets.Site;

                        var response =
                            await client.GetAsync(
                                $"https://{Targets.Site}",
                                token);

                        log.Success(
                            $"{ip} -> {(int)response.StatusCode}");
                    }
                    catch (Exception ex)
                    {
                        log.Warning(
                            $"{ip} -> HTTP FAIL: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
