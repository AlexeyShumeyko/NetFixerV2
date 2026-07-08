using System.Net;

namespace NetFixer.Core
{
    public class DiagnosticContext
    {
        private static readonly Lazy<DiagnosticContext> _instance =
        new(() => new DiagnosticContext());

        public static DiagnosticContext Instance =>
            _instance.Value;

        private DiagnosticContext()
        {
        }

        public string Site { get; private set; } = string.Empty;

        public string HttpsUrl =>
            $"https://{Site}";

        public IPAddress[] ResolvedAddresses { get; private set; } = [];

        public string PublicIp { get; set; } = string.Empty;

        public string Gateway { get; set; } = string.Empty;

        public List<string> DnsServers { get; } = [];

        public string ActiveAdapter { get; set; } = string.Empty;

        public string HttpStatus { get; set; } = string.Empty;

        public string HttpsStatus { get; set; } = string.Empty;

        public string Tcp443Status { get; set; } = string.Empty;

        public string TlsVersion { get; set; } = string.Empty;

        public string MtuResult { get; set; } = string.Empty;

        public string Provider { get; set; } = string.Empty;

        public string OsVersion { get; set; } = string.Empty;

        public string ExternalDnsIp { get; set; } = string.Empty;

        public string TracerouteSummary { get; set; } = string.Empty;

        public string PingSummary { get; set; } = string.Empty;

        public string SiteStatus { get; set; } = string.Empty;

        public bool ProxyDetected { get; set; }

        public bool HostsModified { get; set; }

        public bool FirewallEnabled { get; set; }

        public bool AntivirusDetected { get; set; }

        public bool Tcp80Open { get; set; }

        public bool Tcp443Open { get; set; }

        public bool DnsResolved { get; set; }

        public bool HttpsAvailable { get; set; }

        public bool InternetAvailable { get; set; }

        public async Task InitializeAsync(string site)
        {
            Site = site;

            ResolvedAddresses =
                await Dns.GetHostAddressesAsync(site);
        }
    }
}
