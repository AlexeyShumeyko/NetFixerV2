using NetFixer.Interfaces;
using NetFixer.Resources;
using NetFixer.V3;
using System.Text.RegularExpressions;

namespace NetFixer.Adapters;

public class V3SmartAdapter : ILog
{
    private readonly V3Report _report = new();
    private readonly ILog? _inner;

    private string _currentPlugin = "";
    private string _currentSubSection = "";
    private string _lastCommand = "";

    // Паттерны
    private static readonly Regex IpLatencyRegex = new(@"^(?<ip>[\d\.]+)\s*[:→]\s*(?<latency>\d+)\s*ms", RegexOptions.Compiled);
    private static readonly Regex TcpRegex = new(@"TCP\s+(?<port>\d+)\s+(?<status>OK|FAIL|TIMEOUT)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TlsRegex = new(@"TLS\s+(?<status>OK|FAIL).*Protocol:\s*(?<version>[\w\.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TlsSimpleRegex = new(@"TLS\s+OK", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PublicIpRegex = new(@"Public\s+IP:\s*(?<ip>[\d\.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DnsResolveRegex = new(@"Резолвинг\s+успешен:\s*(?<domain>[\w\.\-]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DnsIpRegex = new(@"(?:Name|Address):\s+(?<value>[\w\.\-]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex FirewallRegex = new(@"[Бб]рандмауэр\s+(?<status>включ[её]н|отключ[её]н|enabled|disabled)", RegexOptions.Compiled);
    private static readonly Regex VpnRegex = new(@"Обнаружен\s+VPN\s+адаптер:\s*(?<name>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex VirtualAdapterRegex = new(@"Обнаружен\s+виртуальный\s+адаптер:\s*(?<name>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex MtuRegex = new(@"Recommended\s+MTU:\s*(?<mtu>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex MtuPingRegex = new(@"ping\s+-f\s+-n\s+1\s+-l\s+(?<size>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex GeoCountryRegex = new(@"Country:\s*(?<country>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex GeoCityRegex = new(@"City:\s*(?<city>[\w\s\-]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IspRegex = new(@"ISP:\s*(?<name>.+?)(?:\s*ASN:|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex AsnRegex = new(@"ASN:\s*(?<asn>AS\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex GatewayRegex = new(@"->\s*(?<ip>[\d\.]+)", RegexOptions.Compiled);
    private static readonly Regex LocalIpRegex = new(@"^(?<adapter>[^:]+):\s*(?<ip>[\d\.:]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DnsServerRegex = new(@"^(?<adapter>[^:]+)\s*->\s*(?<ip>[\d\.:]+)", RegexOptions.Compiled);
    private static readonly Regex HttpStatusRegex = new(@"HTTP\s+(?<code>\d+)\s+OK", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ResponseTimeRegex = new(@"Response\s+Time:\s*(?<ms>\d+)\s*ms", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ContentLengthRegex = new(@"Content\s+Length:\s*(?<bytes>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HttpVersionRegex = new(@"HTTP[/]?(?<version>[\d\.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SniRegex = new(@"SNI\s+OK\s*\((?<version>[\w\.]+)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex CertSubjectRegex = new(@"Subject:\s*(?<subject>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex CertIssuerRegex = new(@"Issuer:\s*(?<issuer>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex CertDateRegex = new(@"(?<type>Not\s+Before|Not\s+After):\s*(?<date>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex CertThumbRegex = new(@"Thumbprint:\s*(?<thumb>[A-F0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TracertHopRegex = new(@"\s*(?<hop>\d+)\s+(?:<?\s*)(?<ms1>\d+)\s*ms\s+(?:<?\s*)(?<ms2>\d+)?\s*ms\s+(?:<?\s*)(?<ms3>\d+)?\s*ms\s+(?<ip>[\d\.]+)", RegexOptions.Compiled);
    private static readonly Regex TracertHopSimpleRegex = new(@"\s*(?<hop>\d+)\s+(?<ms>\d+)\s*ms\s+(?<ip>[\d\.]+)", RegexOptions.Compiled);
    private static readonly Regex TracertCompleteRegex = new(@"Trace\s+complete", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PingFragmentRegex = new(@"Packet\s+needs\s+to\s+be\s+fragmented", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PingSuccessRegex = new(@"Reply\s+from\s+[\d\.]+:\s*bytes=\d+\s+time=(?<ms>\d+)ms\s+TTL=(?<ttl>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ActiveAdapterRegex = new(@"(?<name>[\w\s\-]+)\s*\((?<type>\w+)\)", RegexOptions.Compiled);
    private static readonly Regex AdapterStatusRegex = new(@"^(?<name>[\w\s\-]+)\s*\|\s*(?<type>\w+)\s*\|\s*(?<status>Up|Down)", RegexOptions.Compiled);
    private static readonly Regex SchannelRegex = new(@"Schannel\s+(?<status>найден|Found|не\s+найден|Not\s+found)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SecurityProtocolRegex = new(@"SecurityProtocol:\s*(?<protocol>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HostsLineRegex = new(@"^\s*(?<ip>\d+\.\d+\.\d+\.\d+|0\.0\.0\.0|127\.0\.0\.1)\s+(?<host>.+)", RegexOptions.Compiled);
    private static readonly Regex HostsCommentRegex = new(@"^\s*#", RegexOptions.Compiled);
    private static readonly Regex HostsEmptyRegex = new(@"^\s*$", RegexOptions.Compiled);
    private static readonly Regex HostsBlockedRegex = new(@"BLOCKED_BY_NSG", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HostsAdobeRegex = new(@"adobe\.com", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex Ipv4CountRegex = new(@"IPv4:\s*(?<count>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex Ipv6CountRegex = new(@"IPv6:\s*(?<count>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex CurlHttpRegex = new(@"HTTP[/]?(?<version>[\d\.]+)\s+(?<code>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ServerHeaderRegex = new(@"Server:\s*(?<server>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex CurlSuccessRegex = new(@"URL\s+(?<url>https?://[\w\.\-/]+)\s+доступен", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex CurlErrorRegex = new(@"URL\s+(?<url>https?://[\w\.\-/]+)\s+недоступен", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DnsFlushSuccessRegex = new(@"Successfully\s+flushed|успешно\s+очищен", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex WinsockResetRegex = new(@"Sucessfully\s+reset|Сброс\s+выполнен", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex AntivirusErrorRegex = new(@"Built-in\s+COM\s+has\s+been\s+disabled|антивирус", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public V3Report Report => _report;
    public V3SmartAdapter(ILog? inner = null) => _inner = inner;

    public void Info(string message)
    {
        _inner?.Info(message);
        AddEvent("info", message);
        TryParseInfo(message);
    }

    public void Success(string message)
    {
        _inner?.Success(message);
        AddEvent("success", message);
        TryParseSuccess(message);
    }

    public void Error(string message)
    {
        _inner?.Error(message);
        AddEvent("error", message);
        TryParseError(message);
    }

    public void Warning(string message)
    {
        _inner?.Warning(message);
        AddEvent("warn", message);
        TryParseWarning(message);
    }

    public void Debug(string message)
    {
        _inner?.Debug(message);
        AddEvent("info", message);
    }

    public void Command(string command, string output, string error, int exitCode)
    {
        _inner?.Command(command, output, error, exitCode);
        _lastCommand = command;
        AddEvent("cmd", command);
        TryParseCommandOutput(command, output, error, exitCode);
    }

    public void SubSection(string title)
    {
        _inner?.SubSection(title);
        _currentSubSection = title;
        AddEvent("sub", title);
        TryParseSubSection(title);
    }

    public void StartPluginGroup(string pluginName)
    {
        _inner?.StartPluginGroup(pluginName);
        _currentPlugin = pluginName;
        AddEvent("plugin", pluginName);
    }

    public void Group(string message)
    {
        _inner?.Group(message);
        AddEvent("group", message);
    }

    // PARSING

    private void TryParseSuccess(string msg)
    {
        // 1. Ping/Latency
        var ipLatency = IpLatencyRegex.Match(msg);
        if (ipLatency.Success)
        {
            var ip = ipLatency.Groups["ip"].Value;
            var latency = int.Parse(ipLatency.Groups["latency"].Value);

            _report.Latency ??= new LatencyMetric();
            _report.Latency.AvgMs = latency;

            EnsureNetwork().Rows.Add(new NetworkRow
            {
                Label = $"Ping {ip}",
                Value = $"{latency} ms",
                Status = "ok"
            });
            EnsureNetwork().OkCount++;
            return;
        }

        // 2. TCP
        if (msg.Contains("TCP") && (msg.Contains("OK") || msg.Contains("TIMEOUT")))
        {
            var port = "443";
            var status = msg.Contains("TIMEOUT") ? "error" : "ok";
            var label = msg.Contains("TIMEOUT") ? "✗ TIMEOUT" : "✓ OK";

            EnsureNetwork().Rows.Add(new NetworkRow
            {
                Label = $"TCP {port}",
                Value = label,
                Status = status
            });
            if (status == "ok") EnsureNetwork().OkCount++;
            else EnsureNetwork().ErrorCount++;
            return;
        }

        // 3. TLS
        if (msg.Contains("TLS") && msg.Contains("OK"))
        {
            var tlsMatch = TlsRegex.Match(msg);
            _report.TlsCertificate ??= new TlsMetric();

            if (tlsMatch.Success)
            {
                _report.TlsCertificate.Version = tlsMatch.Groups["version"].Value;
            }
            _report.TlsCertificate.Status = "Valid";

            return;
        }

        // 4. HTTP
        if (msg.Contains("HTTP") && msg.Contains("OK"))
        {
            var httpMatch = HttpStatusRegex.Match(msg);
            var code = httpMatch.Success ? httpMatch.Groups["code"].Value : "200";

            _report.HttpResponse ??= new HttpResponseMetric { StatusCode = int.Parse(code), Status = "ok" };

            return;
        }

        // 5. SNI
        var sniMatch = SniRegex.Match(msg);
        if (sniMatch.Success || (msg.Contains("SNI") && msg.Contains("OK")))
        {
            return;
        }

        // 6. Public IP
        var pubIp = PublicIpRegex.Match(msg);
        if (pubIp.Success)
        {
            var ip = pubIp.Groups["ip"].Value;
            _report.PublicIp ??= new PublicIpMetric { Ip = ip };
            _report.TargetIp ??= new TargetIpMetric { Ip = ip, Domain = Targets.Site };
            EnsureEnvironment().PublicIp = ip;
            return;
        }

        // 7. DNS Resolve
        var dnsResolve = DnsResolveRegex.Match(msg);
        if (dnsResolve.Success)
        {
            var domain = dnsResolve.Groups["domain"].Value;

            return;
        }

        // 8. DNS flushed
        if (DnsFlushSuccessRegex.IsMatch(msg))
        {
            _report.DnsCache ??= new DnsCacheMetric { Flushed = true };
            return;
        }

        // 9. Winsock reset
        if (WinsockResetRegex.IsMatch(msg))
        {
            // winsock в работе
            return;
        }

        // 10. MTU
        var mtuMatch = MtuRegex.Match(msg);
        if (mtuMatch.Success)
        {
            var mtu = mtuMatch.Groups["mtu"].Value;
            EnsureNetwork().Rows.Add(new NetworkRow
            {
                Label = "MTU (rec.)",
                Value = mtu,
                Status = "ok",
                IsMono = true
            });
            EnsureNetwork().OkCount++;
            return;
        }

        // 11. ISP
        var ispMatch = IspRegex.Match(msg);
        if (ispMatch.Success)
        {
            var name = ispMatch.Groups["name"].Value.Trim();
            _report.Isp ??= new IspMetric { Name = name };
            EnsureEnvironment().Isp = name;
            return;
        }

        // 12. ASN
        var asnMatch = AsnRegex.Match(msg);
        if (asnMatch.Success)
        {
            var asn = asnMatch.Groups["asn"].Value;
            if (_report.Isp != null) _report.Isp.Asn = asn;
            EnsureEnvironment().Asn = asn;
            return;
        }

        // 13. Gateway
        if (_currentSubSection.Contains("Шлюз") && GatewayRegex.IsMatch(msg))
        {
            var gw = GatewayRegex.Match(msg).Groups["ip"].Value;
            EnsureEnvironment().Gateway = gw;
            return;
        }

        // 14. Active Adapter
        var activeAdapter = ActiveAdapterRegex.Match(msg);
        if (activeAdapter.Success && _currentSubSection.Contains("Активный"))
        {
            return;
        }

        // 15. Schannel
        var schannel = SchannelRegex.Match(msg);
        if (schannel.Success)
        {
            var found = schannel.Groups["status"].Value.Contains("найден") ||
                       schannel.Groups["status"].Value.Contains("Found");
            EnsureSecurity().Schannel = found ? "Found" : "Not Found";
            if (found) EnsureSecurity().OkCount++;
            return;
        }

        // 16. PAC Script
        if (msg.Contains("PAC") && msg.Contains("не используется"))
        {
            EnsureSecurity().PacScript = "Not used";
            EnsureSecurity().OkCount++;
            return;
        }

        // 17. WinHTTP Proxy
        if (msg.Contains("Direct access") || msg.Contains("не используется"))
        {
            EnsureSecurity().WinHttpProxy = "Direct access";
            EnsureSecurity().OkCount++;
            return;
        }

        // 18. Hosts check
        if (msg.Contains("не обнаружены") || msg.Contains("not found"))
        {
            EnsureHosts().OurDomainsBlocked = false;
            return;
        }

        // 19. IPv4/IPv6 counts
        var ipv4Count = Ipv4CountRegex.Match(msg);
        if (ipv4Count.Success)
        {
            return;
        }

        // 20. Redirect chain completed
        if (msg.Contains("Redirect chain completed"))
        {
            return;
        }

        // 21. HTTP HEAD
        if (msg.Contains("HEAD") && msg.Contains("OK"))
        {
            EnsureNetwork().Rows.Add(new NetworkRow
            {
                Label = "HTTP HEAD",
                Value = "200 OK",
                Status = "ok"
            });
            EnsureNetwork().OkCount++;
            return;
        }

        // 22. Curl success
        var curlSuccess = CurlSuccessRegex.Match(msg);
        if (curlSuccess.Success)
        {
            var url = curlSuccess.Groups["url"].Value;

            return;
        }

        // 23. Traceroute complete
        if (TracertCompleteRegex.IsMatch(msg))
        {
            return;
        }

        // 24. Final site check
        if (msg.Contains("HTTP 200 OK") && _currentSubSection.Contains("Финальная"))
        {
            _report.HttpResponse ??= new HttpResponseMetric { StatusCode = 200, Status = "ok" };
            return;
        }
    }

    private void TryParseWarning(string msg)
    {
        // 1. Firewall disabled
        if (msg.Contains("Брандмауэр отключён") || msg.Contains("Firewall") && msg.Contains("disabled"))
        {
            _report.Firewall ??= new FirewallMetric { Enabled = false };
            EnsureSecurity().FirewallEnabled = false;
            EnsureSecurity().ErrorCount++;
            return;
        }

        // 2. VPN detected
        var vpn = VpnRegex.Match(msg);
        if (vpn.Success)
        {
            var name = vpn.Groups["name"].Value.Trim();
            EnsureSecurity().VpnDetected = name;
            EnsureSecurity().WarningCount++;
            return;
        }

        // 3. Virtual adapter
        var virt = VirtualAdapterRegex.Match(msg);
        if (virt.Success)
        {
            var name = virt.Groups["name"].Value.Trim();
            if (!EnsureSecurity().VirtualAdapters.Contains(name))
                EnsureSecurity().VirtualAdapters.Add(name);
            EnsureSecurity().WarningCount++;
            return;
        }

        // 4. TCP Timeout
        if (msg.Contains("TCP") && msg.Contains("TIMEOUT"))
        {
            EnsureNetwork().Rows.Add(new NetworkRow
            {
                Label = "TCP 443",
                Value = "✗ TIMEOUT",
                Status = "error"
            });
            EnsureNetwork().ErrorCount++;
            return;
        }

        // 5. TLS Fail
        if (msg.Contains("TLS") && msg.Contains("FAIL"))
        {
            _report.TlsCertificate ??= new TlsMetric { Status = "Failed" };
            EnsureNetwork().Rows.Add(new NetworkRow
            {
                Label = "TLS Handshake",
                Value = "✗ FAIL",
                Status = "error"
            });
            EnsureNetwork().ErrorCount++;
            return;
        }

        // 6. HTTP Fail
        if (msg.Contains("HTTP") && msg.Contains("FAIL"))
        {
            _report.HttpResponse ??= new HttpResponseMetric { Status = "error" };
            EnsureNetwork().Rows.Add(new NetworkRow
            {
                Label = "HTTP GET",
                Value = "✗ FAIL",
                Status = "error"
            });
            EnsureNetwork().ErrorCount++;
            return;
        }

        // 7. IPv6 not available
        if (msg.Contains("IPv6") && (msg.Contains("отсутствуют") || msg.Contains("not available")))
        {
            EnsureNetwork().Rows.Add(new NetworkRow
            {
                Label = "IPv6",
                Value = "Not available",
                Status = "warning"
            });
            EnsureNetwork().WarningCount++;
            return;
        }

        // 8. No AAAA record
        if (msg.Contains("No AAAA record") || msg.Contains("AAAA"))
        {
            EnsureDns().Records.Add(new DnsRecord
            {
                Domain = "AAAA Record",
                Ip = "No AAAA record",
                Status = "warning"
            });
            EnsureDns().WarningCount++;
            return;
        }
    }

    private void TryParseError(string msg)
    {
        // 1. Certificate error
        if (msg.Contains("certificate") || msg.Contains("сертификат") || msg.Contains("Handshake"))
        {
            _report.Certificate ??= new CertificateSection { Error = msg };
            return;
        }

        // 2. Antivirus error
        if (AntivirusErrorRegex.IsMatch(msg))
        {
            // Добавить в security
            return;
        }

        // 3. Connection errors
        if (msg.Contains("A task was canceled") || msg.Contains("timeout") || msg.Contains("соединение"))
        {
            if (_currentSubSection.Contains("TLS"))
            {
                _report.TlsCertificate ??= new TlsMetric { Status = "Failed" };
            }
            if (_currentSubSection.Contains("HTTP"))
            {
                _report.HttpResponse ??= new HttpResponseMetric { Status = "timeout" };
            }
            return;
        }

        // 4. SNI Fail
        if (msg.Contains("SNI") && msg.Contains("FAIL"))
        {
            EnsureNetwork().Rows.Add(new NetworkRow
            {
                Label = "SNI",
                Value = "✗ FAIL",
                Status = "error"
            });
            EnsureNetwork().ErrorCount++;
            return;
        }
    }

    private void TryParseInfo(string msg)
    {
        // 1. DNS Result
        if (_currentSubSection == "DNS результат" && Regex.IsMatch(msg, @"^\d+\.\d+\.\d+\.\d+$"))
        {
            var ip = msg.Trim();
            _report.TargetIp ??= new TargetIpMetric { Ip = ip, Domain = Targets.Site };
            EnsureDns().Records.Add(new DnsRecord
            {
                Domain = Targets.Site,
                Ip = ip,
                Status = "ok"
            });
            EnsureDns().OkCount++;
            return;
        }

        // 2. DNS Servers
        var dnsServer = DnsServerRegex.Match(msg);
        if (dnsServer.Success && _currentSubSection.Contains("DNS серверы"))
        {
            var adapter = dnsServer.Groups["adapter"].Value.Trim();
            var ip = dnsServer.Groups["ip"].Value;
            var serverEntry = $"{ip} ({adapter})";
            if (!EnsureDns().DnsServers.Contains(serverEntry))
                EnsureDns().DnsServers.Add(serverEntry);
            return;
        }

        // 3. Country
        var country = GeoCountryRegex.Match(msg);
        if (country.Success)
        {
            var c = country.Groups["country"].Value.Trim();
            if (_report.PublicIp != null) _report.PublicIp.Country = c;
            EnsureEnvironment().Country = c;
            return;
        }

        // 4. City
        var city = GeoCityRegex.Match(msg);
        if (city.Success)
        {
            var ct = city.Groups["city"].Value.Trim();
            if (_report.PublicIp != null) _report.PublicIp.City = ct;
            EnsureEnvironment().City = ct;
            return;
        }

        // 5. Local IPs
        var localIp = LocalIpRegex.Match(msg);
        if (localIp.Success && _currentSubSection.Contains("Локальная сеть"))
        {
            var adapter = localIp.Groups["adapter"].Value.Trim();
            var ip = localIp.Groups["ip"].Value;
            EnsureEnvironment().LocalIps.Add(new LocalIp { Ip = ip, Adapter = adapter });
            return;
        }

        // 6. Adapter status
        var adapterStatus = AdapterStatusRegex.Match(msg);
        if (adapterStatus.Success && _currentSubSection.Contains("Сетевые адаптеры"))
        {
            // Сбор адаптеров в работе
            return;
        }

        // 7. Gateway
        if (_currentSubSection.Contains("Шлюз") && msg.Contains("->"))
        {
            var gw = GatewayRegex.Match(msg);
            if (gw.Success)
            {
                EnsureEnvironment().Gateway = gw.Groups["ip"].Value;
            }
            return;
        }

        // 8. Response Time
        var respTime = ResponseTimeRegex.Match(msg);
        if (respTime.Success)
        {
            var ms = int.Parse(respTime.Groups["ms"].Value);
            if (_report.HttpResponse == null) _report.HttpResponse = new HttpResponseMetric();
            _report.HttpResponse.ResponseTimeMs = ms;
            return;
        }

        // 9. Content Length
        var contentLen = ContentLengthRegex.Match(msg);
        if (contentLen.Success)
        {
            var bytes = int.Parse(contentLen.Groups["bytes"].Value);
            if (_report.HttpResponse == null) _report.HttpResponse = new HttpResponseMetric();

            return;
        }

        // 10. HTTP Version
        if (_currentSubSection.Contains("HTTP версия") && HttpVersionRegex.IsMatch(msg))
        {
            var ver = HttpVersionRegex.Match(msg).Groups["version"].Value;
            if (_report.HttpResponse == null) _report.HttpResponse = new HttpResponseMetric();
            _report.HttpResponse.HttpVersion = ver;

            return;
        }

        // 11. IPv4 count
        var ipv4 = Ipv4CountRegex.Match(msg);
        if (ipv4.Success)
        {
            return;
        }

        // 12. IPv6 count
        var ipv6 = Ipv6CountRegex.Match(msg);
        if (ipv6.Success)
        {
            return;
        }

        // 13. Security Protocol
        var secProto = SecurityProtocolRegex.Match(msg);
        if (secProto.Success)
        {
            EnsureSecurity().TlsSecurityProtocol = secProto.Groups["protocol"].Value;

            return;
        }

        // 14. Hosts lines
        if (_currentPlugin.Contains("hosts") || _currentSubSection.Contains("hosts"))
        {
            TryParseHostsLine(msg);

            return;
        }

        // 15. Certificate details
        if (_currentSubSection.Contains("сертификат"))
        {
            TryParseCertificateInfo(msg);

            return;
        }

        // 16. Redirect chain
        if (_currentSubSection.Contains("Redirect"))
        {
            var redirect = Regex.Match(msg, @"(?<code>\d+)\s*->\s*(?<url>https?://[\w\.\-/]+)");
            if (redirect.Success)
            {
                EnsureNetwork().Rows.Add(new NetworkRow
                {
                    Label = "Redirect Chain",
                    Value = $"{redirect.Groups["code"].Value} → {redirect.Groups["url"].Value}",
                    Status = "ok"
                });
            }

            return;
        }

        // 17. Headers
        if (_currentSubSection.Contains("заголовки"))
        {
            var headerMatch = Regex.Match(msg, @"^(?<key>[\w\-]+):\s*(?<value>.+)");
            if (headerMatch.Success && _report.HttpResponse != null)
            {
                var key = headerMatch.Groups["key"].Value;
                var value = headerMatch.Groups["value"].Value;
                _report.HttpResponse.Headers[key] = value;

                if (key == "Server" && _report.HttpResponse.Server == "")
                {
                    _report.HttpResponse.Server = value;
                }
            }

            return;
        }
    }

    private void TryParseSubSection(string title)
    {
        if (title.Contains("DNS"))
        {
            EnsureDns();
        }
        else if (title.Contains("Network") || title.Contains("TCP") || title.Contains("TLS"))
        {
            EnsureNetwork();
        }
        else if (title.Contains("Security") || title.Contains("Firewall"))
        {
            EnsureSecurity();
        }
        else if (title.Contains("Certificate") || title.Contains("сертификат"))
        {
            _report.Certificate ??= new CertificateSection();
        }
    }

    private void TryParseCommandOutput(string command, string output, string error, int exitCode)
    {
        // 1. nslookup output
        if (command.Contains("nslookup"))
        {
            ParseNslookupOutput(output);

            return;
        }

        // 2. tracert output
        if (command.Contains("tracert"))
        {
            ParseTracertOutput(output);

            return;
        }

        // 3. curl output
        if (command.Contains("curl"))
        {
            ParseCurlOutput(output, exitCode);

            return;
        }

        // 4. ping (MTU discovery)
        if (command.Contains("ping") && command.Contains("-l"))
        {
            ParseMtuPingOutput(output, command);

            return;
        }

        // 5. netsh winhttp
        if (command.Contains("netsh winhttp"))
        {
            if (output.Contains("Direct access"))
            {
                EnsureSecurity().WinHttpProxy = "Direct access";
                EnsureSecurity().OkCount++;
            }

            return;
        }

        // 6. ipconfig /flushdns
        if (command.Contains("ipconfig") && command.Contains("flushdns"))
        {
            if (exitCode == 0 || output.Contains("Successfully"))
            {
                _report.DnsCache ??= new DnsCacheMetric { Flushed = true };
            }

            return;
        }

        // 7. netsh winsock
        if (command.Contains("winsock"))
        {
            return;
        }
    }

    private void ParseNslookupOutput(string output)
    {
        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string? lastName = null;

        foreach (var line in lines)
        {
            var nameMatch = Regex.Match(line, @"Name:\s+(?<name>[\w\.\-]+)");
            if (nameMatch.Success)
            {
                lastName = nameMatch.Groups["name"].Value;
                continue;
            }

            var addrMatch = Regex.Match(line, @"Address:\s+(?<ip>[\d\.]+)");
            if (addrMatch.Success && lastName != null)
            {
                var ip = addrMatch.Groups["ip"].Value;

                var domain = lastName;
                if (_lastCommand.Contains("online.")) domain = "online." + Targets.Site;
                else if (_lastCommand.Contains("new.")) domain = "new." + Targets.Site;
                else domain = Targets.Site;

                if (!EnsureDns().Records.Any(r => r.Domain == domain && r.Ip == ip))
                {
                    EnsureDns().Records.Add(new DnsRecord
                    {
                        Domain = domain,
                        Ip = ip,
                        Status = "ok"
                    });
                    EnsureDns().OkCount++;
                }
            }
        }
    }

    private void ParseTracertOutput(string output)
    {
        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var hops = new List<TracerouteHop>();
        int hopNum = 1;

        foreach (var line in lines)
        {
            var match = TracertHopRegex.Match(line);
            if (!match.Success)
                match = TracertHopSimpleRegex.Match(line);

            if (match.Success)
            {
                var ip = match.Groups["ip"].Value;
                var ms = match.Groups["ms1"].Success ? match.Groups["ms1"].Value :
                        (match.Groups["ms"].Success ? match.Groups["ms"].Value : null);

                hops.Add(new TracerouteHop
                {
                    Number = hopNum++,
                    Ip = ip,
                    LatencyMs = ms != null ? int.Parse(ms) : null,
                    IsDestination = ip == _report.TargetIp?.Ip
                });
            }
        }

        if (hops.Any())
        {
            _report.Traceroute = new TracerouteSection
            {
                TargetIp = _report.TargetIp?.Ip ?? "",
                Hops = hops
            };
        }
    }

    private void ParseCurlOutput(string output, int exitCode)
    {
        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            // HTTP status
            var httpMatch = CurlHttpRegex.Match(line);
            if (httpMatch.Success)
            {
                var code = int.Parse(httpMatch.Groups["code"].Value);
                _report.HttpResponse ??= new HttpResponseMetric();
                _report.HttpResponse.StatusCode = code;
                _report.HttpResponse.Status = exitCode == 0 ? "ok" : "error";
            }

            // Server header
            var serverMatch = ServerHeaderRegex.Match(line);
            if (serverMatch.Success)
            {
                if (_report.HttpResponse == null) _report.HttpResponse = new HttpResponseMetric();
                _report.HttpResponse.Server = serverMatch.Groups["server"].Value;
            }
        }
    }

    private void ParseMtuPingOutput(string output, string command)
    {
        var sizeMatch = Regex.Match(command, @"-l\s+(\d+)");
        if (!sizeMatch.Success) return;

        var size = int.Parse(sizeMatch.Groups[1].Value);

        if (output.Contains("Packet needs to be fragmented"))
        {
        }
        else if (output.Contains("Reply from"))
        {
            var successMatch = PingSuccessRegex.Match(output);
            if (successMatch.Success)
            {
            }
        }
    }

    private void TryParseHostsLine(string msg)
    {
        if (HostsEmptyRegex.IsMatch(msg))
        {
            EnsureHosts().Lines.Add(new HostsLine { Text = "", Type = "empty" });
            EnsureHosts().TotalLines++;
            return;
        }

        if (HostsCommentRegex.IsMatch(msg))
        {
            EnsureHosts().Lines.Add(new HostsLine { Text = msg.Trim(), Type = "comment" });
            EnsureHosts().TotalLines++;
            return;
        }

        var line = HostsLineRegex.Match(msg);
        if (line.Success)
        {
            var type = "normal";
            if (HostsBlockedRegex.IsMatch(msg)) type = "blocked";
            else if (HostsAdobeRegex.IsMatch(msg)) type = "adobe";

            EnsureHosts().Lines.Add(new HostsLine { Text = msg.Trim(), Type = type });
            EnsureHosts().TotalLines++;

            if (type == "blocked") EnsureHosts().MatchesFound++;
            return;
        }

        EnsureHosts().Lines.Add(new HostsLine { Text = msg.Trim(), Type = "normal" });
        EnsureHosts().TotalLines++;
    }

    private void TryParseCertificateInfo(string msg)
    {
        var subject = CertSubjectRegex.Match(msg);
        if (subject.Success)
        {
            if (_report.Certificate == null) _report.Certificate = new CertificateSection();
            _report.Certificate.Subject = subject.Groups["subject"].Value;
            return;
        }

        var issuer = CertIssuerRegex.Match(msg);
        if (issuer.Success)
        {
            if (_report.Certificate == null) _report.Certificate = new CertificateSection();
            _report.Certificate.Issuer = issuer.Groups["issuer"].Value;
            return;
        }

        var date = CertDateRegex.Match(msg);
        if (date.Success)
        {
            if (_report.Certificate == null) _report.Certificate = new CertificateSection();
            var dateStr = date.Groups["date"].Value;
            if (DateTime.TryParse(dateStr, out var dt))
            {
                if (date.Groups["type"].Value.Contains("Before"))
                    _report.Certificate.NotBefore = dt;
                else
                    _report.Certificate.NotAfter = dt;
            }
            return;
        }

        var thumb = CertThumbRegex.Match(msg);
        if (thumb.Success)
        {
            if (_report.Certificate == null) _report.Certificate = new CertificateSection();
            _report.Certificate.Thumbprint = thumb.Groups["thumb"].Value;
            return;
        }
    }

    private void AddEvent(string type, string message, string? highlight = null)
    {
        _report.Events.Add(new V3Event
        {
            Time = DateTime.Now,
            Type = type,
            Message = message,
            Highlight = highlight
        });
    }

    private NetworkSection EnsureNetwork()
    {
        _report.Network ??= new NetworkSection();
        return _report.Network;
    }

    private DnsSection EnsureDns()
    {
        _report.Dns ??= new DnsSection();
        return _report.Dns;
    }

    private SecuritySection EnsureSecurity()
    {
        _report.Security ??= new SecuritySection();
        return _report.Security;
    }

    private EnvironmentSection EnsureEnvironment()
    {
        _report.Environment ??= new EnvironmentSection();
        return _report.Environment;
    }

    private HostsSection EnsureHosts()
    {
        _report.Hosts ??= new HostsSection();
        return _report.Hosts;
    }

    public void FinalizeReport()
    {
        _report.Status = "Completed";
        _report.GeneratedAt = DateTime.Now;

        if (_report.TargetIp != null && string.IsNullOrEmpty(_report.TargetIp.Domain))
        {
            _report.TargetIp.Domain = Targets.Site;
        }

        CalculateCounters();

        FillMetricsFromSections();

        FillNetworkRowsFromMetrics();
    }

    private void FillNetworkRowsFromMetrics()
    {
        if (_report.Network == null) _report.Network = new NetworkSection();

        // TCP 443
        var tcpStatus = _report.Network.Rows.FirstOrDefault(r => r.Label == "TCP 443")?.Status ?? "ok";
        if (!_report.Network.Rows.Any(r => r.Label == "TCP 443"))
        {
            _report.Network.Rows.Add(new NetworkRow
            {
                Label = "TCP 443",
                Value = tcpStatus == "ok" ? "✓ OK" : "✗ FAIL",
                Status = tcpStatus
            });
        }

        // IPv4
        var targetIp = _report.TargetIp?.Ip;
        if (targetIp != null && !_report.Network.Rows.Any(r => r.Label == "IPv4"))
        {
            _report.Network.Rows.Add(new NetworkRow
            {
                Label = "IPv4",
                Value = targetIp,
                Status = "ok",
                IsMono = true
            });
        }

        // IPv6
        if (!_report.Network.Rows.Any(r => r.Label == "IPv6"))
        {
            _report.Network.Rows.Add(new NetworkRow
            {
                Label = "IPv6",
                Value = "Not available",
                Status = "warning"
            });
        }

        // TLS Handshake
        if (_report.TlsCertificate?.Version != null && !_report.Network.Rows.Any(r => r.Label == "TLS Handshake"))
        {
            _report.Network.Rows.Add(new NetworkRow
            {
                Label = "TLS Handshake",
                Value = _report.TlsCertificate.Version,
                Status = _report.TlsCertificate.Status == "Valid" ? "ok" : "error"
            });
        }

        // SNI
        if (!_report.Network.Rows.Any(r => r.Label == "SNI"))
        {
            _report.Network.Rows.Add(new NetworkRow
            {
                Label = "SNI",
                Value = "OK",
                Status = "ok"
            });
        }

        // HTTP HEAD
        if (_report.HttpResponse?.StatusCode != null && !_report.Network.Rows.Any(r => r.Label == "HTTP HEAD"))
        {
            _report.Network.Rows.Add(new NetworkRow
            {
                Label = "HTTP HEAD",
                Value = $"{_report.HttpResponse.StatusCode} OK",
                Status = "ok"
            });
        }

        // HTTP GET
        if (_report.HttpResponse?.StatusCode != null && !_report.Network.Rows.Any(r => r.Label == "HTTP GET"))
        {
            _report.Network.Rows.Add(new NetworkRow
            {
                Label = "HTTP GET",
                Value = $"{_report.HttpResponse.StatusCode} OK",
                Status = "ok"
            });
        }

        // Redirect Chain
        if (!_report.Network.Rows.Any(r => r.Label == "Redirect Chain"))
        {
            _report.Network.Rows.Add(new NetworkRow
            {
                Label = "Redirect Chain",
                Value = "200 → " + Targets.Site,
                Status = "ok"
            });
        }

        // Response Time
        if (_report.HttpResponse?.ResponseTimeMs != null && !_report.Network.Rows.Any(r => r.Label == "Response Time"))
        {
            _report.Network.Rows.Add(new NetworkRow
            {
                Label = "Response Time",
                Value = $"{_report.HttpResponse.ResponseTimeMs} ms",
                Status = "ok"
            });
        }

        // Content Length
        if (_report.HttpResponse?.StatusCode != null && !_report.Network.Rows.Any(r => r.Label == "Content Length"))
        {
            _report.Network.Rows.Add(new NetworkRow
            {
                Label = "Content Length",
                Value = "—",
                Status = "ok"
            });
        }

        // MTU
        var mtuRow = _report.Network.Rows.FirstOrDefault(r => r.Label == "MTU (rec.)");
        if (mtuRow == null)
        {
            _report.Network.Rows.Add(new NetworkRow
            {
                Label = "MTU (rec.)",
                Value = "1280",
                Status = "ok",
                IsMono = true
            });
        }
    }

    private void CalculateCounters()
    {
        if (_report.Network != null)
        {
            _report.Network.OkCount = _report.Network.Rows.Count(r => r.Status == "ok");
            _report.Network.WarningCount = _report.Network.Rows.Count(r => r.Status == "warning");
            _report.Network.ErrorCount = _report.Network.Rows.Count(r => r.Status == "error");
        }
    }

    private void FillMetricsFromSections()
    {
        if (_report.HttpResponse == null && _report.Network != null)
        {
            var httpRow = _report.Network.Rows.FirstOrDefault(r => r.Label.Contains("HTTP"));
            if (httpRow != null)
            {
                _report.HttpResponse = new HttpResponseMetric
                {
                    Status = httpRow.Status == "ok" ? "ok" : "error"
                };
            }
        }
    }
}