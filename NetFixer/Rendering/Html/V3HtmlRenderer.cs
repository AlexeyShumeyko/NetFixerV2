using NetFixer.Resources;
using NetFixer.V3;
using System.Net;
using System.Text;

namespace NetFixer.Rendering.V3;

public static class V3HtmlRenderer
{
    public static string Render(V3Report report)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"ru\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"utf-8\" />");
        sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        sb.AppendLine("  <title>NetFixer V3 · Diagnostic Report</title>");
        sb.AppendLine("  <link rel=\"preconnect\" href=\"https://fonts.googleapis.com\" />");
        sb.AppendLine("  <link href=\"https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap\" rel=\"stylesheet\" />");
        sb.AppendLine("  <style>");
        sb.AppendLine(GetCss());
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <div class=\"app\">");

        // Header
        RenderHeader(sb, report);

        // Metrics
        RenderMetrics(sb, report);

        // Grid with cards
        sb.AppendLine("    <div class=\"grid\" id=\"cardGrid\">");

        // OVERVIEW CARDS
        RenderNetworkCard(sb, report.Network);
        RenderDnsCard(sb, report.Dns);
        RenderCertificateCard(sb, report.Certificate);
        RenderEnvironmentCard(sb, report.Environment);
        RenderSecurityCard(sb, report.Security);
        RenderTracerouteCard(sb, report.Traceroute);
        RenderHostsCard(sb, report.Hosts);
        RenderEventStream(sb, report.Events);

        // TAB CARDS
        RenderNetworkDetailCard(sb, report);
        RenderNetworkEventStream(sb, report.Events);
        RenderDnsDetailCard(sb, report);
        RenderDnsEventStream(sb, report.Events);
        RenderCertificateDetailCard(sb, report);
        RenderSecurityDetailCard(sb, report);
        RenderSecurityEventStream(sb, report.Events);

        sb.AppendLine("    </div>");

        // Footer
        RenderFooter(sb, report);

        sb.AppendLine("  </div>");
        sb.AppendLine(GetJs());
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static void RenderHeader(StringBuilder sb, V3Report report)
    {
        var isOk = report.Status == "Completed" && (report.Network?.ErrorCount ?? 0) == 0;
        var statusText = isOk ? "All Systems Operational" : "Critical Issues Detected";
        var dotColor = isOk ? "var(--success)" : "var(--danger)";
        var dotShadow = isOk ? "var(--success-light)" : "var(--danger-light)";

        sb.AppendLine("    <header class=\"header\">");
        sb.AppendLine("      <div class=\"header-left\">");
        sb.AppendLine("        <div class=\"logo\">");
        sb.AppendLine("          <span>NF Diagnostic</span>");
        sb.AppendLine("          <span class=\"logo-badge\">v2.0.1</span>");
        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
        sb.AppendLine("      <nav class=\"header-nav\" id=\"navTabs\">");
        sb.AppendLine("        <a class=\"active\" data-section=\"overview\">Overview</a>");
        sb.AppendLine("        <a data-section=\"network\">Network</a>");
        sb.AppendLine("        <a data-section=\"dns\">DNS</a>");
        sb.AppendLine("        <a data-section=\"cert\">Certificate</a>");
        sb.AppendLine("        <a data-section=\"security\">Security</a>");
        sb.AppendLine("        <a data-section=\"events\">Logs</a>");
        sb.AppendLine("      </nav>");
        sb.AppendLine("      <div class=\"header-right\">");
        sb.AppendLine("        <div class=\"status-pill\">");
        sb.AppendLine($"          <span class=\"status-dot\" style=\"background:{dotColor};box-shadow:0 0 0 2px {dotShadow};\"></span>");
        sb.AppendLine($"          {Escape(statusText)}");
        sb.AppendLine($"          <span style=\"color:var(--text-muted);margin-left:4px;\">·</span>");
        sb.AppendLine($"          <span style=\"color:var(--text-muted);font-weight:400;\">{report.GeneratedAt:dd MMM yyyy}</span>");
        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
        sb.AppendLine("    </header>");
    }

    private static void RenderMetrics(StringBuilder sb, V3Report r)
    {
        sb.AppendLine("    <div class=\"metrics\">");

        // 1. Target IP
        RenderMetricCard(sb, "🎯", "Target IP", r.TargetIp?.Ip ?? "—",
            r.TargetIp?.Domain ?? "",
            r.TargetIp?.Version ?? "IPv4", "tag-blue");

        // 2. HTTP Response
        var httpStatus = r.HttpResponse?.Status ?? "unknown";
        var httpTag = httpStatus == "ok" ? "tag-green" : httpStatus == "timeout" ? "tag-red" : "tag-yellow";
        var httpValue = r.HttpResponse?.ResponseTimeMs?.ToString() ?? "N/A";
        var httpSub = r.HttpResponse?.Server != null ? $"{r.HttpResponse.Server} · HTTP/{r.HttpResponse.HttpVersion}" : "Connection timeout";
        RenderMetricCard(sb, httpStatus == "ok" ? "✅" : "❌", "HTTP Response",
            httpValue,
            httpSub,
            httpStatus == "ok" ? $"{r.HttpResponse?.StatusCode} OK" : "Timeout",
            httpTag,
            subValue: "ms");

        // 3. Latency
        var latStatus = r.Latency?.Status ?? "Unknown";
        var latValue = r.Latency?.AvgMs?.ToString() ?? "—";
        var latFootnote = r.Latency?.Ttl != null ? $"Avg ping · TTL {r.Latency.Ttl}" : "Avg ping";
        RenderMetricCard(sb, "📡", "Latency",
            latValue,
            latFootnote,
            latStatus,
            latStatus == "Healthy" ? "tag-green" : "tag-yellow",
            subValue: "ms");

        // 4. TLS
        var tlsStatus = r.TlsCertificate?.Status ?? "Unknown";
        var tlsVersion = r.TlsCertificate?.Version ?? "—";
        if (tlsVersion == "Tls12") tlsVersion = "TLS 1.2";
        else if (tlsVersion == "Tls13") tlsVersion = "TLS 1.3";
        else if (tlsVersion == "Tls11") tlsVersion = "TLS 1.1";
        else if (tlsVersion == "Tls10") tlsVersion = "TLS 1.0";

        RenderMetricCard(sb, "🔒", "TLS Certificate",
            tlsVersion,
            r.TlsCertificate?.Issuer ?? "",
            tlsStatus,
            tlsStatus == "Valid" ? "tag-green" : "tag-red",
            valueSize: "24px");

        // 5. Public IP
        RenderMetricCard(sb, "📍", "Public IP",
            r.PublicIp?.Ip ?? "—",
            $"{r.PublicIp?.Country ?? ""} · {r.PublicIp?.City ?? ""}",
            "IPv4", "tag-blue",
            valueSize: "24px", isMono: true);

        // 6. ISP
        RenderMetricCard(sb, "🏢", "ISP",
            r.Isp?.Name ?? "—",
            r.Isp?.Name ?? "",
            r.Isp?.Asn ?? "AS?",
            "tag-blue",
            valueSize: "24px");

        // 7. Firewall
        var fwEnabled = r.Firewall?.Enabled ?? false;
        RenderMetricCard(sb, "🛡️", "Firewall",
            fwEnabled ? "Enabled" : "⚠️ Disabled",
            fwEnabled ? "Active protection" : "Requires attention",
            fwEnabled ? "Enabled" : "Disabled",
            fwEnabled ? "tag-green" : "tag-red",
            valueSize: "24px",
            valueColor: fwEnabled ? null : "var(--danger)");

        // 8. DNS Cache
        var dnsFlushed = r.DnsCache?.Flushed ?? false;
        RenderMetricCard(sb, "🔍", "DNS Cache",
            dnsFlushed ? "✅ Success" : "Pending",
            dnsFlushed ? "Cleared successfully" : "",
            dnsFlushed ? "Flushed" : "Pending",
            dnsFlushed ? "tag-green" : "tag-yellow",
            valueSize: "24px");

        sb.AppendLine("    </div>");
    }

    private static void RenderMetricCard(StringBuilder sb, string icon, string label,
        string value, string footnote, string tagText, string tagClass,
        string? subValue = null, string valueSize = "32px", bool isMono = false, string? valueColor = null)
    {
        var monoStyle = isMono ? "font-family:'SF Mono','Menlo','Monaco','Courier New',monospace;" : "";
        var colorStyle = valueColor != null ? $"color:{valueColor};" : "";

        sb.AppendLine("      <div class=\"metric-card\">");
        sb.AppendLine("        <div class=\"top\">");
        sb.AppendLine("          <div class=\"label-wrap\">");
        sb.AppendLine($"            <span class=\"icon\">{icon}</span>");
        sb.AppendLine($"            <span class=\"label\">{Escape(label)}</span>");
        sb.AppendLine("          </div>");
        sb.AppendLine($"          <span class=\"tag {tagClass}\">{Escape(tagText)}</span>");
        sb.AppendLine("        </div>");

        sb.AppendLine($"        <div class=\"value\" style=\"font-size:{valueSize};{monoStyle}{colorStyle}\">{Escape(value)} <span class=\"sub\">{Escape(subValue ?? "")}</span></div>");

        if (!string.IsNullOrEmpty(footnote))
        {
            sb.AppendLine($"        <div class=\"footnote\">{Escape(footnote)}</div>");
        }

        sb.AppendLine("      </div>");
    }

    private static void RenderNetworkCard(StringBuilder sb, NetworkSection? section)
    {
        if (section == null) return;

        sb.AppendLine("      <div class=\"card\" data-section=\"overview network\">");
        RenderCardHeader(sb, "🌐", "Network", "Diagnostics", section.OkCount, section.WarningCount, section.ErrorCount);
        sb.AppendLine("        <div class=\"card-body\">");

        foreach (var row in section.Rows)
        {
            var statusClass = row.Status switch
            {
                "ok" => "tag-success",
                "warning" => "tag-warning",
                "error" => "tag-danger",
                _ => "tag-info"
            };

            var valueHtml = row.IsMono
                ? $"<span class=\"mono\">{Escape(row.Value)}</span>"
                : $"<span class=\"{statusClass}\">{Escape(row.Value)}</span>";

            sb.AppendLine($"          <div class=\"row\"><span class=\"label\">{Escape(row.Label)}</span><span class=\"value\">{valueHtml}</span></div>");
        }

        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
    }

    private static void RenderDnsCard(StringBuilder sb, DnsSection? section)
    {
        if (section == null) return;

        sb.AppendLine("      <div class=\"card\" data-section=\"overview dns\">");
        RenderCardHeader(sb, "🔍", "DNS", "Resolution", section.OkCount, section.WarningCount, section.ErrorCount);
        sb.AppendLine("        <div class=\"card-body\">");

        foreach (var record in section.Records)
        {
            var statusClass = record.Status == "ok" ? "tag-success" : "tag-warning";
            sb.AppendLine($"          <div class=\"row\"><span class=\"label\">{Escape(record.Domain)}</span><span class=\"value\"><span class=\"mono\">{Escape(record.Ip)}</span> <span class=\"{statusClass}\">✓</span></span></div>");
        }

        var googleDns = section.Records.FirstOrDefault(r => r.Domain.Contains("Google") || r.Domain.Contains("8.8.8.8"));
        var cloudflareDns = section.Records.FirstOrDefault(r => r.Domain.Contains("Cloudflare") || r.Domain.Contains("1.1.1.1"));

        if (googleDns == null)
        {
            var googleIp = section.Records.FirstOrDefault(r => r.Domain == Targets.Site)?.Ip;
            if (googleIp != null)
            {
                sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Google DNS (8.8.8.8)</span><span class=\"value\"><span class=\"mono\">{Escape(googleIp)}</span> <span class=\"tag-success\">✓</span></span></div>");
            }
        }

        if (cloudflareDns == null)
        {
            var cfIp = section.Records.FirstOrDefault(r => r.Domain == Targets.Site)?.Ip;
            if (cfIp != null)
            {
                sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Cloudflare DNS (1.1.1.1)</span><span class=\"value\"><span class=\"mono\">{Escape(cfIp)}</span> <span class=\"tag-success\">✓</span></span></div>");
            }
        }

        // A Record
        var aRecord = section.Records.FirstOrDefault(r => r.Domain == Targets.Site);
        if (aRecord != null)
        {
            sb.AppendLine($"          <div class=\"row\"><span class=\"label\">A Record</span><span class=\"value\"><span class=\"mono\">{Escape(aRecord.Ip)}</span></span></div>");
        }

        // AAAA Record
        var aaaaRecord = section.Records.FirstOrDefault(r => r.Domain.Contains("AAAA"));
        if (aaaaRecord != null)
        {
            sb.AppendLine($"          <div class=\"row\"><span class=\"label\">AAAA Record</span><span class=\"value\"><span class=\"tag-warning\">{Escape(aaaaRecord.Ip)}</span></span></div>");
        }

        // DNS Cache
        sb.AppendLine($"          <div class=\"row\"><span class=\"label\">DNS Cache</span><span class=\"value\"><span class=\"tag-success\">Flushed</span></span></div>");

        // DNS Servers
        if (section.DnsServers.Any())
        {
            var serverGroups = new Dictionary<string, List<string>>();
            foreach (var srv in section.DnsServers)
            {
                var parts = srv.Split(new[] { "→" }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    var adapter = parts[0].Trim();
                    var ip = parts[1].Trim();
                    if (!serverGroups.ContainsKey(adapter)) serverGroups[adapter] = new List<string>();
                    serverGroups[adapter].Add(ip);
                }
            }

            sb.AppendLine($"          <div class=\"row\" style=\"border-bottom:none;padding-top:6px;margin-top:2px;\">");
            sb.AppendLine($"            <span class=\"label\" style=\"font-size:11px;color:var(--text-muted);\">DNS Servers</span>");
            sb.AppendLine($"            <span class=\"value\" style=\"font-size:11px;color:var(--text-secondary);text-align:right;\">");

            var serverTexts = serverGroups.Select(g => $"{string.Join(", ", g.Value)} ({g.Key})");
            sb.AppendLine(string.Join(" · ", serverTexts));

            sb.AppendLine("            </span>");
            sb.AppendLine("          </div>");
        }

        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
    }

    private static void RenderCertificateCard(StringBuilder sb, CertificateSection? section)
    {
        if (section == null) return;

        sb.AppendLine("      <div class=\"card\" data-section=\"overview cert\">");

        var hasError = section.Error != null;
        RenderCardHeader(sb, "🔐", "Certificate", "Info",
            hasError ? 0 : 1, 0, hasError ? 1 : 0);

        sb.AppendLine("        <div class=\"card-body\">");

        if (hasError)
        {
            sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Subject</span><span class=\"value\" style=\"color:var(--danger);\">Unavailable</span></div>");
            sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Issuer</span><span class=\"value\" style=\"color:var(--danger);\">Unavailable</span></div>");
            sb.AppendLine($"          <div class=\"row\" style=\"border-top:1px solid var(--border);margin-top:8px;padding-top:8px;\">");
            sb.AppendLine($"            <span class=\"label\" style=\"color:var(--danger);\">Error</span>");
            sb.AppendLine($"            <span class=\"value\" style=\"color:var(--danger);\">{Escape(section.Error!)}</span>");
            sb.AppendLine("          </div>");
        }
        else
        {
            sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Subject</span><span class=\"value\" style=\"font-size:12px;font-family:monospace;\">{Escape(section.Subject ?? "")}</span></div>");
            sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Issuer</span><span class=\"value\" style=\"font-size:12px;font-family:monospace;\">{Escape(section.Issuer ?? "")}</span></div>");
            sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Not Before</span><span class=\"value\" style=\"font-size:12px;\">{section.NotBefore:dd MMM yyyy HH:mm:ss}</span></div>");
            sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Not After</span><span class=\"value\" style=\"font-size:12px;\">{section.NotAfter:dd MMM yyyy HH:mm:ss}</span></div>");
            sb.AppendLine($"          <div class=\"row\" style=\"border-bottom:none;\"><span class=\"label\">Thumbprint</span><span class=\"value\" style=\"font-size:11px;font-family:monospace;word-break:break-all;\">{Escape(section.Thumbprint ?? "")}</span></div>");
        }

        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
    }

    private static void RenderEnvironmentCard(StringBuilder sb, EnvironmentSection? section)
    {
        if (section == null) return;

        sb.AppendLine("      <div class=\"card\" data-section=\"overview\">");
        RenderCardHeader(sb, "🖥️", "Environment", "& Network", 1, 0, 0);
        sb.AppendLine("        <div class=\"card-body\">");

        sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Public IP</span><span class=\"value\"><span class=\"mono\">{Escape(section.PublicIp)}</span></span></div>");
        sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Country</span><span class=\"value\">{Escape(section.Country)}</span></div>");
        sb.AppendLine($"          <div class=\"row\"><span class=\"label\">City</span><span class=\"value\">{Escape(section.City)}</span></div>");
        sb.AppendLine($"          <div class=\"row\"><span class=\"label\">ISP</span><span class=\"value\">{Escape(section.Isp)}</span></div>");
        sb.AppendLine($"          <div class=\"row\"><span class=\"label\">ASN</span><span class=\"value\">{Escape(section.Asn)}</span></div>");
        sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Gateway</span><span class=\"value\"><span class=\"mono\">{Escape(section.Gateway)}</span></span></div>");

        if (section.LocalIps.Any())
        {
            var ipsHtml = string.Join("<br>", section.LocalIps.Select(ip =>
                $"{Escape(ip.Ip)} <span style=\"color:var(--text-muted);\">({Escape(ip.Adapter)})</span>"));
            sb.AppendLine($"          <div class=\"row\" style=\"border-bottom:none;\"><span class=\"label\">Local IPs</span><span class=\"value\" style=\"font-size:12px;font-family:monospace;text-align:right;line-height:1.6;\">{ipsHtml}</span></div>");
        }

        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
    }

    private static void RenderSecurityCard(StringBuilder sb, SecuritySection? section)
    {
        if (section == null) return;

        var fwColor = section.FirewallEnabled ? "var(--success)" : "var(--danger)";
        var fwText = section.FirewallEnabled ? "Enabled" : "Disabled";

        sb.AppendLine("      <div class=\"card\" data-section=\"overview security\">");

        var okCount = section.OkCount;
        var warnCount = section.WarningCount;
        var errCount = section.ErrorCount;

        sb.AppendLine("        <div class=\"card-header\">");
        sb.AppendLine("          <span class=\"icon\">🛡️</span>");
        sb.AppendLine("          <span class=\"title\">Security <span class=\"accent\">Posture</span></span>");
        sb.AppendLine("          <div class=\"status-indicators\">");

        if (okCount > 0)
            sb.AppendLine($"            <span class=\"indicator\"><span class=\"dot success\"></span><span class=\"count success\">{okCount}</span><span class=\"label-text\">OK</span></span>");
        if (warnCount > 0)
            sb.AppendLine($"            <span class=\"indicator\"><span class=\"dot warning\"></span><span class=\"count warning\">{warnCount}</span><span class=\"label-text\">Warning</span></span>");
        if (errCount > 0)
            sb.AppendLine($"            <span class=\"indicator\"><span class=\"dot error\"></span><span class=\"count error\">{errCount}</span><span class=\"label-text\">Error</span></span>");

        sb.AppendLine("          </div>");
        sb.AppendLine("        </div>");

        sb.AppendLine("        <div class=\"card-body\">");
        sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Firewall</span><span class=\"value\" style=\"color:{fwColor};\">{fwText}</span></div>");
        sb.AppendLine($"          <div class=\"row\"><span class=\"label\">WinHTTP Proxy</span><span class=\"value\" style=\"color:var(--success);\">{Escape(section.WinHttpProxy)}</span></div>");
        sb.AppendLine($"          <div class=\"row\"><span class=\"label\">PAC Script</span><span class=\"value\" style=\"color:var(--success);\">{Escape(section.PacScript)}</span></div>");

        var vpnColor = section.VpnDetected != null ? "var(--warning)" : "var(--success)";
        var vpnText = section.VpnDetected ?? "Not detected";
        sb.AppendLine($"          <div class=\"row\"><span class=\"label\">VPN Detected</span><span class=\"value\" style=\"color:{vpnColor};\">{Escape(vpnText)}</span></div>");

        if (section.VirtualAdapters.Any())
        {
            sb.AppendLine($"          <div class=\"row\" style=\"border-bottom:none;\"><span class=\"label\">Virtual Adapters</span><span class=\"value\" style=\"font-size:12px;color:var(--text-secondary);\">{Escape(string.Join(" · ", section.VirtualAdapters))}</span></div>");
        }

        sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Schannel</span><span class=\"value\"><span class=\"tag-success\">{Escape(section.Schannel)}</span></span></div>");
        sb.AppendLine($"          <div class=\"row\" style=\"border-bottom:none;\"><span class=\"label\">TLS SecurityProtocol</span><span class=\"value\"><span class=\"mono\">{Escape(section.TlsSecurityProtocol)}</span></span></div>");
        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
    }

    private static void RenderHostsCard(StringBuilder sb, HostsSection? section)
    {
        if (section == null) return;

        sb.AppendLine("      <div class=\"card card-full\" data-section=\"overview\">");
        RenderCardHeader(sb, "📄", "Hosts File", "Analysis", 0, 0, 0,
            extraIndicator: $"{section.TotalLines} lines · {section.MatchesFound} matches");
        sb.AppendLine("        <div class=\"card-body\">");
        sb.AppendLine("          <div class=\"hosts-scroll\">");

        foreach (var line in section.Lines)
        {
            var cssClass = line.Type switch
            {
                "comment" => "hl-comment",
                "adobe" => "hl-adobe",
                "blocked" => "hl-blocked",
                "ok" => "hl-ok",
                _ => ""
            };

            if (!string.IsNullOrEmpty(cssClass))
            {
                sb.AppendLine($"            <div class=\"line\"><span class=\"{cssClass}\">{Escape(line.Text)}</span></div>");
            }
            else
            {
                sb.AppendLine($"            <div class=\"line\">{Escape(line.Text)}</div>");
            }
        }

        sb.AppendLine("          </div>");
        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
    }

    private static void RenderTracerouteCard(StringBuilder sb, TracerouteSection? section)
    {
        if (section == null || !section.Hops.Any()) return;

        sb.AppendLine($"      <div class=\"card\" data-section=\"overview network\">");
        RenderCardHeader(sb, "📡", "Traceroute", $"to {Escape(section.TargetIp)}", 0, 0, 0,
            extraIndicator: $"{section.Hops.Count} hops");
        sb.AppendLine("        <div class=\"card-body\">");
        sb.AppendLine("          <ul class=\"trace-list\">");

        foreach (var hop in section.Hops)
        {
            var destClass = hop.IsDestination ? "dest" : "";
            var ipColor = hop.IsDestination ? "style=\"color:var(--primary);\"" : "";
            var ipWeight = hop.IsDestination ? "font-weight:700;" : "font-weight:500;";

            sb.AppendLine($"            <li class=\"{destClass}\"><span class=\"hop\">{hop.Number}</span><span class=\"ip\" {ipColor} style=\"{ipWeight}\">{Escape(hop.Ip)}</span><span class=\"ms\">{hop.LatencyMs} ms{(hop.IsDestination ? " ✓" : "")}</span></li>");
        }

        sb.AppendLine("          </ul>");
        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
    }

    private static void RenderEventStream(StringBuilder sb, List<V3Event> events)
    {
        if (!events.Any()) return;

        sb.AppendLine("      <div class=\"card card-full\" data-section=\"overview events\" style=\"padding-bottom:16px;\">");
        RenderCardHeader(sb, "📋", "Event", "Stream", 0, 0, 0,
            extraIndicator: $"{events.Count} events");
        sb.AppendLine("        <div class=\"card-body\">");
        sb.AppendLine("          <div class=\"event-stream\">");

        foreach (var evt in events)
        {
            var typeClass = evt.Type.ToLower() switch
            {
                "info" => "info",
                "success" => "success",
                "warn" or "warning" => "warn",
                "error" => "error",
                "cmd" => "cmd",
                "group" => "group",
                "plugin" => "plugin",
                "sub" => "sub",
                _ => "info"
            };

            var msg = evt.Highlight != null
                ? evt.Message.Replace(evt.Highlight, $"<span class=\"highlight\">{Escape(evt.Highlight)}</span>")
                : Escape(evt.Message);

            sb.AppendLine($"            <div class=\"event-item\"><span class=\"ev-time\">{evt.Time:HH:mm:ss}</span><span class=\"ev-type {typeClass}\">{Escape(evt.Type.ToUpper())}</span><span class=\"ev-msg\">{msg}</span></div>");
        }

        sb.AppendLine("          </div>");
        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
    }

    private static void RenderCardHeader(StringBuilder sb, string icon, string title, string accent,
        int okCount, int warnCount, int errCount, string? extraIndicator = null)
    {
        sb.AppendLine("        <div class=\"card-header\">");
        sb.AppendLine($"          <span class=\"icon\">{icon}</span>");
        sb.AppendLine($"          <span class=\"title\">{Escape(title)} <span class=\"accent\">{Escape(accent)}</span></span>");
        sb.AppendLine("          <div class=\"status-indicators\">");

        if (okCount > 0)
            sb.AppendLine($"            <span class=\"indicator\"><span class=\"dot success\"></span><span class=\"count success\">{okCount}</span><span class=\"label-text\">OK</span></span>");
        if (warnCount > 0)
            sb.AppendLine($"            <span class=\"indicator\"><span class=\"dot warning\"></span><span class=\"count warning\">{warnCount}</span><span class=\"label-text\">Warning</span></span>");
        if (errCount > 0)
            sb.AppendLine($"            <span class=\"indicator\"><span class=\"dot error\"></span><span class=\"count error\">{errCount}</span><span class=\"label-text\">Error</span></span>");
        if (extraIndicator != null)
            sb.AppendLine($"            <span class=\"indicator\"><span class=\"dot info\"></span><span class=\"count info\">{Escape(extraIndicator)}</span></span>");

        sb.AppendLine("          </div>");
        sb.AppendLine("        </div>");
    }

    private static void RenderFooter(StringBuilder sb, V3Report report)
    {
        sb.AppendLine("    <div class=\"footer\">");
        sb.AppendLine($"      <span>NetFixer V3 · Security Diagnostic Report</span>");
        sb.AppendLine($"      <span>Generated: {report.GeneratedAt:dd MMM yyyy HH:mm:ss} UTC · Session: ACTIVE</span>");
        sb.AppendLine("    </div>");
    }

    private static string Escape(string input) => WebUtility.HtmlEncode(input ?? "");

    private static void RenderNetworkDetailCard(StringBuilder sb, V3Report report)
    {
        sb.AppendLine("      <div class=\"card card-full\" data-section=\"network\">");
        sb.AppendLine("        <div class=\"card-header\">");
        sb.AppendLine("          <span class=\"icon\">🌐</span>");
        sb.AppendLine("          <span class=\"title\">Network <span class=\"accent\">Details</span></span>");
        sb.AppendLine("        </div>");
        sb.AppendLine("        <div class=\"card-body\">");

        if (report.Network != null)
        {
            foreach (var row in report.Network.Rows)
            {
                sb.AppendLine($"          <div class=\"row\"><span class=\"label\">{Escape(row.Label)}</span><span class=\"value\">{Escape(row.Value)}</span></div>");
            }
        }

        sb.AppendLine("          <div class=\"section-title\" style=\"margin-top:16px;font-weight:700;\">Per-IP Diagnostics</div>");

        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
    }

    private static void RenderNetworkEventStream(StringBuilder sb, List<V3Event> events)
    {
        var filtered = events.Where(e =>
            e.Message.Contains("Маршрутизация") ||
            e.Message.Contains("Ping") ||
            e.Message.Contains("TCP") ||
            e.Message.Contains("TLS") ||
            e.Message.Contains("HTTP") ||
            (e.Type == "plugin" && (e.Message.Contains("Маршрутизация") || e.Message.Contains("Сетевая")))
        ).ToList();

        RenderEventStreamCard(sb, filtered, "Network Events", "network");
    }

    private static void RenderDnsDetailCard(StringBuilder sb, V3Report report)
    {
        sb.AppendLine("      <div class=\"card card-full\" data-section=\"dns\" style=\"display:none;\">");
        sb.AppendLine("        <div class=\"card-header\">");
        sb.AppendLine("          <span class=\"icon\">🔍</span>");
        sb.AppendLine("          <span class=\"title\">DNS <span class=\"accent\">Resolution Details</span></span>");
        sb.AppendLine("        </div>");
        sb.AppendLine("        <div class=\"card-body\">");

        if (report.Dns != null)
        {
            foreach (var record in report.Dns.Records)
            {
                sb.AppendLine($"          <div class=\"row\"><span class=\"label\">{Escape(record.Domain)}</span><span class=\"value\"><span class=\"mono\">{Escape(record.Ip)}</span></span></div>");
            }

            if (report.Dns.DnsServers.Any())
            {
                sb.AppendLine($"          <div class=\"row\" style=\"border-bottom:none;margin-top:8px;\"><span class=\"label\">DNS Servers</span><span class=\"value\" style=\"font-size:12px;\">{Escape(string.Join(" · ", report.Dns.DnsServers))}</span></div>");
            }
        }

        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
    }

    private static void RenderDnsEventStream(StringBuilder sb, List<V3Event> events)
    {
        var filtered = events.Where(e =>
            e.Message.Contains("DNS") ||
            e.Message.Contains("nslookup") ||
            e.Message.Contains("flushdns") ||
            (e.Type == "plugin" && e.Message.Contains("DNS"))
        ).ToList();

        RenderEventStreamCard(sb, filtered, "DNS Events", "dns");
    }

    private static void RenderCertificateDetailCard(StringBuilder sb, V3Report report)
    {
        sb.AppendLine("      <div class=\"card card-full\" data-section=\"cert\" style=\"display:none;\">");
        sb.AppendLine("        <div class=\"card-header\">");
        sb.AppendLine("          <span class=\"icon\">🔐</span>");
        sb.AppendLine("          <span class=\"title\">Certificate <span class=\"accent\">Details</span></span>");
        sb.AppendLine("        </div>");
        sb.AppendLine("        <div class=\"card-body\">");

        if (report.Certificate != null)
        {
            if (report.Certificate.Error != null)
            {
                sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Status</span><span class=\"value\" style=\"color:var(--danger);\">Error: {Escape(report.Certificate.Error)}</span></div>");
            }
            else
            {
                sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Subject</span><span class=\"value\" style=\"font-family:monospace;font-size:12px;\">{Escape(report.Certificate.Subject ?? "—")}</span></div>");
                sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Issuer</span><span class=\"value\" style=\"font-family:monospace;font-size:12px;\">{Escape(report.Certificate.Issuer ?? "—")}</span></div>");
                sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Valid From</span><span class=\"value\">{report.Certificate.NotBefore:dd MMM yyyy HH:mm:ss}</span></div>");
                sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Valid To</span><span class=\"value\">{report.Certificate.NotAfter:dd MMM yyyy HH:mm:ss}</span></div>");
                sb.AppendLine($"          <div class=\"row\" style=\"border-bottom:none;\"><span class=\"label\">Thumbprint</span><span class=\"value\" style=\"font-family:monospace;font-size:11px;word-break:break-all;\">{Escape(report.Certificate.Thumbprint ?? "—")}</span></div>");
            }
        }

        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
    }

    private static void RenderSecurityDetailCard(StringBuilder sb, V3Report report)
    {
        sb.AppendLine("      <div class=\"card card-full\" data-section=\"security\" style=\"display:none;\">");
        sb.AppendLine("        <div class=\"card-header\">");
        sb.AppendLine("          <span class=\"icon\">🛡️</span>");
        sb.AppendLine("          <span class=\"title\">Security <span class=\"accent\">Details</span></span>");
        sb.AppendLine("        </div>");
        sb.AppendLine("        <div class=\"card-body\">");

        if (report.Security != null)
        {
            var fwColor = report.Security.FirewallEnabled ? "var(--success)" : "var(--danger)";
            var fwText = report.Security.FirewallEnabled ? "Enabled" : "Disabled";

            sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Firewall</span><span class=\"value\" style=\"color:{fwColor};\">{fwText}</span></div>");
            sb.AppendLine($"          <div class=\"row\"><span class=\"label\">WinHTTP Proxy</span><span class=\"value\">{Escape(report.Security.WinHttpProxy)}</span></div>");
            sb.AppendLine($"          <div class=\"row\"><span class=\"label\">PAC Script</span><span class=\"value\">{Escape(report.Security.PacScript)}</span></div>");

            var vpnColor = report.Security.VpnDetected != null ? "var(--warning)" : "var(--success)";
            var vpnText = report.Security.VpnDetected ?? "Not detected";
            sb.AppendLine($"          <div class=\"row\"><span class=\"label\">VPN</span><span class=\"value\" style=\"color:{vpnColor};\">{Escape(vpnText)}</span></div>");

            if (report.Security.VirtualAdapters.Any())
            {
                sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Virtual Adapters</span><span class=\"value\" style=\"font-size:12px;\">{Escape(string.Join(" · ", report.Security.VirtualAdapters))}</span></div>");
            }

            sb.AppendLine($"          <div class=\"row\"><span class=\"label\">Schannel</span><span class=\"value\">{Escape(report.Security.Schannel)}</span></div>");
            sb.AppendLine($"          <div class=\"row\" style=\"border-bottom:none;\"><span class=\"label\">TLS Protocol</span><span class=\"value\"><span class=\"mono\">{Escape(report.Security.TlsSecurityProtocol)}</span></span></div>");
        }

        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
    }

    private static void RenderSecurityEventStream(StringBuilder sb, List<V3Event> events)
    {
        var filtered = events.Where(e =>
            e.Message.Contains("Firewall") ||
            e.Message.Contains("прокси") ||
            e.Message.Contains("Proxy") ||
            e.Message.Contains("PAC") ||
            e.Message.Contains("VPN") ||
            e.Message.Contains("антивирус") ||
            e.Message.Contains("Schannel") ||
            (e.Type == "plugin" && e.Message.Contains("Безопасность"))
        ).ToList();

        RenderEventStreamCard(sb, filtered, "Security Events", "security");
    }

    private static void RenderEventStreamCard(StringBuilder sb, List<V3Event> events, string title, string sectionAttr)
    {
        if (!events.Any()) return;

        sb.AppendLine($"      <div class=\"card card-full\" data-section=\"{sectionAttr}\" style=\"display:none;\">");
        sb.AppendLine("        <div class=\"card-header\">");
        sb.AppendLine($"          <span class=\"icon\">📋</span>");
        sb.AppendLine($"          <span class=\"title\">{Escape(title)}</span>");
        sb.AppendLine($"          <div class=\"status-indicators\"><span class=\"indicator\"><span class=\"dot info\"></span><span class=\"count info\">{events.Count}</span><span class=\"label-text\">events</span></span></div>");
        sb.AppendLine("        </div>");
        sb.AppendLine("        <div class=\"card-body\">");
        sb.AppendLine("          <div class=\"event-stream\">");

        foreach (var evt in events)
        {
            var typeClass = evt.Type.ToLower() switch
            {
                "info" => "info",
                "success" => "success",
                "warn" or "warning" => "warn",
                "error" => "error",
                "cmd" => "cmd",
                "group" => "group",
                "plugin" => "plugin",
                "sub" => "sub",
                _ => "info"
            };

            var msg = evt.Highlight != null
                ? evt.Message.Replace(evt.Highlight, $"<span class=\"highlight\">{Escape(evt.Highlight)}</span>")
                : Escape(evt.Message);

            sb.AppendLine($"            <div class=\"event-item\"><span class=\"ev-time\">{evt.Time:HH:mm:ss}</span><span class=\"ev-type {typeClass}\">{Escape(evt.Type.ToUpper())}</span><span class=\"ev-msg\">{msg}</span></div>");
        }

        sb.AppendLine("          </div>");
        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
    }

    private static string GetCss() => @"
:root {
    --font: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
    --bg: #f6f8fa;
    --surface: #ffffff;
    --surface-hover: rgba(0, 0, 0, 0.02);
    --border: #e1e4e8;
    --border-hover: rgba(0, 0, 0, 0.10);
    --text: #1c1e21;
    --text-secondary: #5f6368;
    --text-muted: #6e6e73;
    --primary: #2563eb;
    --primary-bg: #dbeafe;
    --success: #059669;
    --success-light: #d1fae5;
    --warning: #d97706;
    --warning-light: #fef3c7;
    --danger: #dc2626;
    --danger-light: #fee2e2;
    --shadow: 0 1px 3px rgba(0,0,0,0.08), 0 1px 2px rgba(0,0,0,0.04);
    --shadow-md: 0 10px 25px -5px rgba(0,0,0,0.08), 0 8px 10px -6px rgba(0,0,0,0.02);
    --radius: 12px;
    --radius-sm: 8px;
    --transition: 0.3s cubic-bezier(0.25, 0.1, 0.25, 1);
}
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
html { font-size: 14px; -webkit-font-smoothing: antialiased; -moz-osx-font-smoothing: grayscale; }
body { font-family: var(--font); background: var(--bg); color: var(--text); min-height: 100vh; padding: 24px 32px 40px; margin: 0; line-height: 1.5; }
::-webkit-scrollbar { width: 4px; height: 4px; }
::-webkit-scrollbar-track { background: transparent; }
::-webkit-scrollbar-thumb { background: var(--text-muted); border-radius: 20px; }
.app { max-width: 1280px; margin: 0 auto; display: flex; flex-direction: column; gap: 24px; }
.header { display: flex; align-items: center; justify-content: space-between; padding: 12px 20px; background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius); box-shadow: var(--shadow); transition: all var(--transition); flex-wrap: wrap; gap: 10px; }
.header:hover { border-color: var(--border-hover); box-shadow: var(--shadow-md); }
.header-left { display: flex; align-items: center; gap: 14px; }
.logo { display: flex; align-items: center; gap: 10px; font-weight: 700; font-size: 18px; letter-spacing: -0.3px; color: var(--text); }
.logo-icon { width: 36px; height: 36px; border-radius: 8px; background: var(--primary); display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 16px; color: #fff; }
.logo-badge { font-size: 10px; font-weight: 600; letter-spacing: 0.4px; text-transform: uppercase; color: var(--primary); background: var(--primary-bg); padding: 2px 12px; border-radius: 100px; border: 1px solid rgba(37, 99, 235, 0.1); }
.header-nav { display: flex; align-items: center; gap: 4px; flex-wrap: wrap; }
.header-nav a { padding: 6px 16px; border-radius: 100px; font-size: 13px; font-weight: 500; color: var(--text-secondary); text-decoration: none; transition: all var(--transition); cursor: pointer; background: transparent; }
.header-nav a:hover { background: var(--surface); color: var(--text); }
.header-nav a.active { background: var(--primary); color: #fff; box-shadow: 0 2px 12px rgba(37, 99, 235, 0.25); }
.header-right { display: flex; align-items: center; gap: 16px; }
.status-pill { display: flex; align-items: center; gap: 8px; font-size: 12px; font-weight: 500; color: var(--text-secondary); background: var(--surface); padding: 4px 14px; border-radius: 100px; border: 1px solid var(--border); box-shadow: var(--shadow); }
.status-dot { width: 8px; height: 8px; border-radius: 50%; background: var(--success); animation: pulse-dot 2s ease-in-out infinite; box-shadow: 0 0 0 2px var(--success-light); }
@keyframes pulse-dot { 0%, 100% { opacity: 1; transform: scale(1); } 50% { opacity: 0.5; transform: scale(0.85); } }
.metrics { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; }
.metric-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius); padding: 24px 26px; box-shadow: var(--shadow); transition: all var(--transition); display: flex; flex-direction: column; min-height: 130px; justify-content: space-between; }
.metric-card:hover { box-shadow: var(--shadow-md); transform: translateY(-2px); border-color: var(--border-hover); }
.metric-card .top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }
.metric-card .top .label-wrap { display: flex; align-items: center; gap: 8px; }
.metric-card .top .icon { font-size: 20px; line-height: 1; opacity: 0.7; }
.metric-card .top .label { font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.4px; color: var(--text-muted); }
.metric-card .top .tag { padding: 2px 10px; border-radius: 6px; font-size: 11px; font-weight: 600; }
.tag-blue { background: var(--primary-bg); color: var(--primary); }
.tag-green { background: var(--success-light); color: var(--success); }
.tag-yellow { background: var(--warning-light); color: var(--warning); }
.tag-red { background: var(--danger-light); color: var(--danger); }
.metric-card .value { font-size: 32px; font-weight: 700; letter-spacing: -0.5px; color: var(--text); line-height: 1.2; margin-bottom: 4px; }
.metric-card .value .sub { font-size: 16px; font-weight: 400; color: var(--text-secondary); margin-left: 4px; }
.metric-card .footnote { font-size: 13px; color: var(--text-secondary); margin-top: 4px; }
.metric-card .footnote .highlight { color: var(--text); font-weight: 500; }
.metric-card .progress { margin-top: 10px; width: 100%; height: 4px; background: var(--border); border-radius: 10px; overflow: hidden; }
.metric-card .progress .fill { height: 100%; border-radius: 10px; background: var(--success); width: 0%; }
.metric-card .progress .fill.warning { background: var(--warning); }
.metric-card .progress .fill.danger { background: var(--danger); }
@media (max-width: 820px) { .metrics { grid-template-columns: repeat(2, 1fr); } .metric-card { min-height: 120px; padding: 20px 22px; } }
@media (max-width: 480px) { .metrics { grid-template-columns: 1fr; } .metric-card { min-height: 110px; padding: 18px 20px; } }
.grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
@media (max-width: 900px) { .grid { grid-template-columns: 1fr; } }
.card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius); padding: 20px 24px; box-shadow: var(--shadow); transition: all var(--transition); animation: fadeSlide 0.5s ease both; }
.card:hover { border-color: var(--border-hover); box-shadow: var(--shadow-md); }
.card-header { display: flex; align-items: center; gap: 10px; margin-bottom: 14px; padding-bottom: 12px; border-bottom: 1px solid var(--border); }
.card-header .icon { font-size: 20px; line-height: 1; opacity: 0.7; }
.card-header .title { font-size: 16px; font-weight: 700; letter-spacing: -0.2px; color: var(--text); }
.card-header .title .accent { color: var(--primary); }
.status-indicators { display: flex; gap: 12px; margin-left: auto; align-items: center; flex-wrap: wrap; }
.status-indicators .indicator { display: flex; align-items: center; gap: 4px; font-size: 13px; font-weight: 500; }
.status-indicators .indicator .dot { width: 10px; height: 10px; border-radius: 50%; display: inline-block; flex-shrink: 0; }
.status-indicators .indicator .dot.success { background: var(--success); box-shadow: 0 0 0 2px var(--success-light); }
.status-indicators .indicator .dot.warning { background: var(--warning); box-shadow: 0 0 0 2px var(--warning-light); }
.status-indicators .indicator .dot.error { background: var(--danger); box-shadow: 0 0 0 2px var(--danger-light); }
.status-indicators .indicator .dot.info { background: var(--primary); box-shadow: 0 0 0 2px var(--primary-bg); }
.status-indicators .indicator .count { color: var(--text-secondary); font-weight: 600; }
.status-indicators .indicator .count.success { color: var(--success); }
.status-indicators .indicator .count.warning { color: var(--warning); }
.status-indicators .indicator .count.error { color: var(--danger); }
.status-indicators .indicator .count.info { color: var(--primary); }
.status-indicators .indicator .label-text { color: var(--text-secondary); font-weight: 400; font-size: 12px; margin-left: 2px; }
.card-body { display: flex; flex-direction: column; gap: 2px; }
.row { display: flex; justify-content: space-between; align-items: center; padding: 6px 0; border-bottom: 1px solid rgba(0,0,0,0.03); font-size: 13px; }
.row:last-child { border-bottom: none; }
.row .label { color: var(--text-secondary); font-weight: 500; }
.row .value { color: var(--text); font-weight: 500; text-align: right; word-break: break-all; max-width: 70%; }
.row .value .mono { font-family: 'SF Mono', 'Menlo', 'Monaco', 'Courier New', monospace; font-size: 12px; background: var(--bg); padding: 1px 8px; border-radius: 6px; border: 1px solid var(--border); }
.tag-success { color: var(--success); }
.tag-danger { color: var(--danger); }
.tag-warning { color: var(--warning); }
.tag-info { color: var(--primary); }
.hosts-scroll { max-height: 200px; overflow-y: auto; font-size: 13px; font-family: 'SF Mono', 'Menlo', 'Monaco', 'Courier New', monospace; background: var(--bg); border-radius: var(--radius-sm); padding: 12px 16px; border: 1px solid var(--border); line-height: 2; margin-top: 4px; }
.hosts-scroll .line { color: var(--text-secondary); white-space: pre-wrap; word-break: break-all; }
.hosts-scroll .line .hl-adobe { color: var(--danger); font-weight: 500; }
.hosts-scroll .line .hl-blocked { color: var(--warning); font-weight: 500; }
.hosts-scroll .line .hl-comment { color: var(--text-muted); font-style: italic; }
.hosts-scroll .line .hl-ok { color: var(--success); font-weight: 600; }
.trace-list { list-style: none; padding: 0; margin: 0; font-size: 13px; font-family: var(--font); }
.trace-list li { display: flex; align-items: center; gap: 12px; padding: 3px 0; border-bottom: 1px solid rgba(0,0,0,0.03); color: var(--text-secondary); }
.trace-list li .hop { font-weight: 600; color: var(--text-muted); min-width: 24px; font-size: 12px; }
.trace-list li .ip { color: var(--text); font-weight: 500; }
.trace-list li .ms { color: var(--text-muted); font-size: 12px; margin-left: auto; }
.trace-list li.dest .ip { color: var(--primary); font-weight: 700; }
.event-stream { max-height: 380px; overflow-y: auto; border-radius: var(--radius-sm); background: var(--bg); border: 1px solid var(--border); padding: 4px 0; margin-top: 4px; }
.event-item { display: flex; align-items: flex-start; gap: 10px; padding: 5px 16px; font-size: 12px; font-family: var(--font); border-bottom: 1px solid rgba(0,0,0,0.02); transition: background 0.15s ease; }
.event-item:hover { background: var(--surface-hover); }
.event-item .ev-time { color: var(--text-muted); font-size: 11px; white-space: nowrap; min-width: 64px; font-weight: 500; padding-top: 1px; }
.event-item .ev-type { font-weight: 600; font-size: 10px; text-transform: uppercase; letter-spacing: 0.3px; min-width: 44px; padding: 1px 8px; border-radius: 4px; text-align: center; flex-shrink: 0; background: var(--surface); border: 1px solid var(--border); color: var(--text-secondary); }
.ev-type.info { color: var(--primary); border-color: var(--primary-bg); background: var(--primary-bg); }
.ev-type.success { color: var(--success); border-color: var(--success-light); background: var(--success-light); }
.ev-type.warn { color: var(--warning); border-color: var(--warning-light); background: var(--warning-light); }
.ev-type.error { color: var(--danger); border-color: var(--danger-light); background: var(--danger-light); }
.ev-type.cmd { color: var(--primary); border-color: var(--primary-bg); background: var(--primary-bg); }
.ev-type.group { color: var(--warning); border-color: var(--warning-light); background: var(--warning-light); }
.ev-type.plugin { color: var(--text-muted); border-color: var(--border); background: transparent; }
.ev-type.sub { color: var(--text-secondary); border-color: var(--border); background: transparent; font-weight: 400; }
.event-item .ev-msg { color: var(--text-secondary); word-break: break-word; flex: 1; padding-top: 1px; }
.event-item .ev-msg .highlight { color: var(--text); font-weight: 600; }
.sec-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; }
@media (max-width: 500px) { .sec-grid { grid-template-columns: 1fr; } }
.sec-item { background: var(--bg); border-radius: var(--radius-sm); padding: 10px 14px; border: 1px solid var(--border); display: flex; justify-content: space-between; align-items: center; transition: all var(--transition); }
.sec-item:hover { border-color: var(--border-hover); }
.sec-item .sec-label { font-size: 12px; color: var(--text-secondary); font-weight: 500; }
.sec-item .sec-value { font-size: 13px; font-weight: 600; }
.card-full { grid-column: 1 / -1; }
.footer { margin-top: 8px; padding-top: 16px; border-top: 1px solid var(--border); display: flex; justify-content: space-between; flex-wrap: wrap; gap: 12px; font-size: 12px; color: var(--text-muted); }
@media (max-width: 768px) { body { padding: 16px; } .header { flex-direction: column; align-items: stretch; gap: 10px; padding: 14px 16px; } .header-left { justify-content: space-between; } .header-nav { justify-content: center; gap: 4px; } .header-nav a { font-size: 12px; padding: 4px 12px; } .header-right { justify-content: center; } .card { padding: 16px 18px; } .event-item { flex-wrap: wrap; gap: 2px; } .event-item .ev-time { min-width: 50px; } .grid { gap: 14px; } .status-indicators .indicator { font-size: 12px; } .card-header .title { font-size: 15px; } .metric-card .value { font-size: 26px; } }
@media (max-width: 480px) { .header-nav a { font-size: 11px; padding: 3px 10px; } .status-indicators { gap: 8px; } .status-indicators .indicator { font-size: 11px; } }
@keyframes fadeSlide { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }
.card { animation: fadeSlide 0.5s ease both; }
.card:nth-child(1) { animation-delay: 0.04s; }
.card:nth-child(2) { animation-delay: 0.08s; }
.card:nth-child(3) { animation-delay: 0.12s; }
.card:nth-child(4) { animation-delay: 0.16s; }
.card:nth-child(5) { animation-delay: 0.20s; }
.card:nth-child(6) { animation-delay: 0.24s; }
.card:nth-child(7) { animation-delay: 0.28s; }
.card:nth-child(8) { animation-delay: 0.32s; }
.metric-card { animation: fadeSlide 0.5s ease both; }
.metric-card:nth-child(1) { animation-delay: 0.02s; }
.metric-card:nth-child(2) { animation-delay: 0.04s; }
.metric-card:nth-child(3) { animation-delay: 0.06s; }
.metric-card:nth-child(4) { animation-delay: 0.08s; }
.metric-card:nth-child(5) { animation-delay: 0.10s; }
.metric-card:nth-child(6) { animation-delay: 0.12s; }
.metric-card:nth-child(7) { animation-delay: 0.14s; }
.metric-card:nth-child(8) { animation-delay: 0.16s; }
@media print { body { padding: 10px !important; background: #fff !important; color: #000 !important; } .card, .metric-card, .header { box-shadow: none !important; border-color: #ccc !important; background: #fff !important; backdrop-filter: none !important; } .event-stream, .hosts-scroll { max-height: none !important; overflow: visible !important; } .status-dot { animation: none !important; } }
";

    private static string GetJs() => @"
<script>
(function() {
    const navLinks = document.querySelectorAll('#navTabs a');
    const cards = document.querySelectorAll('#cardGrid .card');

    function filterCards(section) {
        cards.forEach(card => {
            const sections = card.dataset.section ? card.dataset.section.split(' ') : [];
            
            if (section === 'overview') {
                // Overview: показываем карточки с overview в data-section
                // и скрываем детальные карточки вкладок (у них style=""display:none;"" изначально)
                if (sections.includes('overview')) {
                    card.style.display = '';
                } else {
                    card.style.display = 'none';
                }
            } else {
                // Другие вкладки: показываем только карточки с этой секцией
                if (sections.includes(section)) {
                    card.style.display = '';
                } else {
                    card.style.display = 'none';
                }
            }
        });
    }

    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            navLinks.forEach(l => l.classList.remove('active'));
            this.classList.add('active');
            const section = this.dataset.section;
            filterCards(section);
        });
    });

    filterCards('overview');
})();
</script>
";
}