namespace NetFixer.V3;

public class V3Report
{
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public string Status { get; set; } = "Running";

    // KPI Metrics
    public TargetIpMetric? TargetIp { get; set; }
    public HttpResponseMetric? HttpResponse { get; set; }
    public LatencyMetric? Latency { get; set; }
    public TlsMetric? TlsCertificate { get; set; }
    public PublicIpMetric? PublicIp { get; set; }
    public IspMetric? Isp { get; set; }
    public FirewallMetric? Firewall { get; set; }
    public DnsCacheMetric? DnsCache { get; set; }

    // Sections
    public NetworkSection? Network { get; set; }
    public DnsSection? Dns { get; set; }
    public CertificateSection? Certificate { get; set; }
    public EnvironmentSection? Environment { get; set; }
    public SecuritySection? Security { get; set; }
    public HostsSection? Hosts { get; set; }
    public TracerouteSection? Traceroute { get; set; }

    // Event Stream
    public List<V3Event> Events { get; set; } = new();
}

// Metrics
public class TargetIpMetric
{
    public string Ip { get; set; } = "";
    public string Domain { get; set; } = "";
    public string Version { get; set; } = "IPv4";
    public string Status { get; set; } = "ok";
}

public class HttpResponseMetric
{
    public int? StatusCode { get; set; }
    public int? ResponseTimeMs { get; set; }
    public string Server { get; set; } = "";
    public string HttpVersion { get; set; } = "";
    public string Status { get; set; } = "ok";
    public int? ContentLength { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
}

public class LatencyMetric
{
    public int? AvgMs { get; set; }
    public int? Ttl { get; set; }
    public string Status { get; set; } = "Healthy";
}

public class TlsMetric
{
    public string? Version { get; set; }
    public string? Issuer { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string Status { get; set; } = "Valid";
}

public class PublicIpMetric
{
    public string Ip { get; set; } = "";
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
}

public class IspMetric
{
    public string Name { get; set; } = "";
    public string Asn { get; set; } = "";
}

public class FirewallMetric
{
    public bool Enabled { get; set; }
    public string StatusText => Enabled ? "Enabled" : "Disabled";
}

public class DnsCacheMetric
{
    public bool Flushed { get; set; }
    public string StatusText => Flushed ? "Flushed" : "Not flushed";
}

// Sections
public class NetworkSection
{
    public List<NetworkRow> Rows { get; set; } = new();
    public int OkCount { get; set; }
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }
}

public class NetworkRow
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string Status { get; set; } = "ok";
    public bool IsMono { get; set; }
}

public class DnsSection
{
    public List<DnsRecord> Records { get; set; } = new();
    public List<string> DnsServers { get; set; } = new();
    public int OkCount { get; set; }
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }
}

public class DnsRecord
{
    public string Domain { get; set; } = "";
    public string Ip { get; set; } = "";
    public string Status { get; set; } = "ok";
}

public class CertificateSection
{
    public string? Subject { get; set; }
    public string? Issuer { get; set; }
    public DateTime? NotBefore { get; set; }
    public DateTime? NotAfter { get; set; }
    public string? Thumbprint { get; set; }
    public string? Error { get; set; }
    public string Status => Error == null ? "Valid" : "Error";
}

public class EnvironmentSection
{
    public string PublicIp { get; set; } = "";
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
    public string Isp { get; set; } = "";
    public string Asn { get; set; } = "";
    public string Gateway { get; set; } = "";
    public List<LocalIp> LocalIps { get; set; } = new();
}

public class LocalIp
{
    public string Ip { get; set; } = "";
    public string Adapter { get; set; } = "";
}

public class SecuritySection
{
    public bool FirewallEnabled { get; set; }
    public string WinHttpProxy { get; set; } = "Direct access";
    public string PacScript { get; set; } = "Not used";
    public string? VpnDetected { get; set; }
    public List<string> VirtualAdapters { get; set; } = new();
    public string Schannel { get; set; } = "Found";
    public string TlsSecurityProtocol { get; set; } = "SystemDefault";
    public int OkCount { get; set; }
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }
}

public class HostsSection
{
    public List<HostsLine> Lines { get; set; } = new();
    public int TotalLines { get; set; }
    public int MatchesFound { get; set; }
    public bool OurDomainsBlocked { get; set; }
}

public class HostsLine
{
    public string Text { get; set; } = "";
    public string Type { get; set; } = "normal";
}

public class TracerouteSection
{
    public string TargetIp { get; set; } = "";
    public List<TracerouteHop> Hops { get; set; } = new();
}

public class TracerouteHop
{
    public int Number { get; set; }
    public string Ip { get; set; } = "";
    public int? LatencyMs { get; set; }
    public bool IsDestination { get; set; }
}

// Event Stream
public class V3Event
{
    public DateTime Time { get; set; }
    public string Type { get; set; } = "";
    public string Message { get; set; } = "";
    public string? Highlight { get; set; }
}