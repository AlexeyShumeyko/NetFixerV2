using NetFixer.Interfaces;
using System.Net.NetworkInformation;

namespace NetFixer.Plugins.Environment
{
    public class VpnDetectionPlugin : INetFixPlugin
    {
        private static readonly string[] VpnKeywords =
            [
                "vpn",
                "wireguard",
                "openvpn",
                "nord",
                "express",
                "proton",
                "warp",
                "tunnel",
                "tap",
                "tun",
                "cloudflare",
                "zerotier",
                "tailscale",
                "amnezia",
                "goodbyeDPI",
                "adguard"
            ];

        public string Name => "VPN диагностика";

        public async Task ExecuteAsync(
            ILog log,
            CancellationToken token)
        {
            log.SubSection(Name);

            try
            {
                var found = false;

                foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
                {
                    var lower =
                        adapter.Name.ToLowerInvariant();

                    if (VpnKeywords.Any(
                        x => lower.Contains(x)))
                    {
                        found = true;

                        log.Warning(
                            $"Обнаружен VPN адаптер: {adapter.Name}");
                    }
                }

                if (!found)
                {
                    log.Success(
                        "VPN адаптеры не обнаружены.");
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
