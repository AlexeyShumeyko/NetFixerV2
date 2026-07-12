using NetFixer.Interfaces;
using NetFixer.Utils;
using System.Management;
using System.Text.RegularExpressions;

namespace NetFixer.Plugins.Security
{
    public class AntivirusCheckPlugin : INetFixPlugin
    {
        public string Name => "Проверка антивирусов";

        public async Task ExecuteAsync(ILog log, CancellationToken token)
        {
            log.SubSection(Name);

            var wmiSuccess = await TryWmiAsync(log, token);
            if (wmiSuccess) return;

            var psSuccess = await TryPowerShellAsync(log, token);
            if (psSuccess) return;

            await TryProcessCheckAsync(log, token);
        }

        private async Task<bool> TryWmiAsync(ILog log, CancellationToken token)
        {
            try
            {
                var searcher = new ManagementObjectSearcher(
                    @"root\SecurityCenter2",
                    "SELECT * FROM AntivirusProduct");

                var found = false;
                foreach (ManagementObject obj in searcher.Get())
                {
                    found = true;
                    var displayName = obj["displayName"]?.ToString();
                    var productState = obj["productState"] != null ? Convert.ToUInt32(obj["productState"]) : 0U;

                    var status = GetAvStatus(productState);
                    if (status == "Enabled")
                        log.Success($"Антивирус: {displayName} ({status})");
                    else if (status == "Disabled")
                        log.Warning($"Антивирус: {displayName} ({status})");
                    else
                        log.Info($"Антивирус: {displayName} ({status})");
                }

                if (!found)
                {
                    log.Warning("Антивирусы не обнаружены через WMI.");
                    return false;
                }

                return true;
            }
            catch (TypeInitializationException)
            {
                log.Info("WMI недоступен в данном режиме запуска.");
                return false;
            }
            catch (Exception ex)
            {
                log.Info($"WMI ошибка: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> TryPowerShellAsync(ILog log, CancellationToken token)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(10000);

                var result = await CommandExecutor.ExecuteAsync(
                    "powershell -ExecutionPolicy Bypass -Command \"Get-CimInstance -Namespace root/SecurityCenter2 -ClassName AntivirusProduct | Select-Object displayName, productState | Format-Table -AutoSize\"",
                    log,
                    logOutput: false,
                    logError: false,
                    cts.Token);

                if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(result.Output))
                    return false;

                var lines = result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var found = false;

                foreach (var line in lines.Skip(2))
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.Contains("---")) continue;

                    var match = Regex.Match(trimmed, @"^(.+?)\s+(\d+)$");
                    if (match.Success)
                    {
                        found = true;
                        var name = match.Groups[1].Value.Trim();
                        var state = uint.Parse(match.Groups[2].Value);
                        var status = GetAvStatus(state);

                        if (status == "Enabled")
                            log.Success($"Антивирус: {name} ({status})");
                        else if (status == "Disabled")
                            log.Warning($"Антивирус: {name} ({status})");
                        else
                            log.Info($"Антивирус: {name} ({status})");
                    }
                    else if (!trimmed.Contains("displayName") && trimmed.Length > 3)
                    {
                        found = true;
                        log.Info($"Антивирус: {trimmed}");
                    }
                }

                if (!found)
                {
                    log.Warning("Антивирусы не обнаружены через PowerShell.");
                    return false;
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                log.Warning("PowerShell таймаут при проверке антивирусов.");
                return false;
            }
            catch (Exception ex)
            {
                log.Info($"PowerShell ошибка: {ex.Message}");
                return false;
            }
        }

        private async Task TryProcessCheckAsync(ILog log, CancellationToken token)
        {
            try
            {
                var knownAvProcesses = new[]
                {
                    "MsMpEng", "MsSense", "SecurityHealthService",        // Windows Defender
                    "avp", "avpui", "kavfs",                              // Kaspersky
                    "ekrn", "egui",                                       // ESET
                    "avastsvc", "avastui",                                // Avast
                    "avgsvc", "avgui",                                    // AVG
                    "mcshield", "mcapexe", "mcsvhost",                    // McAfee
                    "bdagent", "vsserv",                                  // BitDefender
                    "ccsvchst", "nortonsecurity", "ns",                   // Norton
                    "sophos", "swc_service",                              // Sophos
                    "cb", "carbonblack",                                  // Carbon Black
                    "sentinelone", "s1-agent",                            // SentinelOne
                    "crowdstrike", "csfalcon",                            // CrowdStrike
                    "cybereason", "crsvc",                                // Cybereason
                    "mbam", "mbamservice",                                // Malwarebytes
                    "drweb", "spider",                                    // Dr.Web
                    "360rp", "360safe",                                   // 360 Total Security
                    "qqpcrtp", "qqpc",                                    // Tencent
                };

                var found = false;
                foreach (var proc in System.Diagnostics.Process.GetProcesses())
                {
                    try
                    {
                        var procName = proc.ProcessName.ToLowerInvariant();
                        if (knownAvProcesses.Any(av => procName.Contains(av.ToLowerInvariant())))
                        {
                            found = true;
                            log.Info($"Обнаружен процесс антивируса: {proc.ProcessName}");
                        }
                    }
                    catch { }
                }

                if (!found)
                {
                    log.Warning("Антивирусы не обнаружены ни одним из методов.");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private static string GetAvStatus(uint productState)
        {
            var hex = productState.ToString("X6");
            var realTimeProtection = hex.Substring(2, 2);
            var definitionStatus = hex.Substring(4, 2);

            if (realTimeProtection == "10" || realTimeProtection == "11")
                return "Enabled";
            if (realTimeProtection == "00" || realTimeProtection == "01")
                return "Disabled";

            return "Unknown";
        }
    }
}