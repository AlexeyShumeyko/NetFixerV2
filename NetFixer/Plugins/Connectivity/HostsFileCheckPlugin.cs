using NetFixer.Interfaces;

namespace NetFixer.Plugins.Connectivity
{
    public class HostsFileCheckPlugin : INetFixPlugin
    {
        public string Name => "Проверка содержимого hosts";

        private readonly List<string> _ourDomains = new()
        {
            "fabrika-fotoknigi.com",
            "online.fabrika-fotoknigi.com",
        };

        public async Task ExecuteAsync(ILog log, CancellationToken cancellationToken)
        {
            log.StartPluginGroup(Name);

            var path = @"C:\Windows\System32\drivers\etc\hosts";

            try
            {
                if (!File.Exists(path))
                {
                    log.Error("Файл hosts не найден.");

                    return;
                }

                log.Info("Чтение файла hosts...");

                var lines = await File.ReadAllLinesAsync(path);
                var foundOurDamains = new List<string>();

                if (lines.Length == 0)
                {
                    log.Info("Файл hosts пуст.");

                    return;
                }

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();

                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    {
                        log.SubSection($" {line}");
                        continue;
                    }

                    foreach (var domain in _ourDomains)
                    {
                        if (line.Contains(domain, StringComparison.OrdinalIgnoreCase))
                        {
                            foundOurDamains.Add(domain);
                            log.Error($" {line}");

                            break;
                        }
                    }

                    if (!foundOurDamains.Any(d => line.Contains(d, StringComparison.OrdinalIgnoreCase)))
                        log.SubSection($" {line}");
                }

                //Анализ
                if (foundOurDamains.Count > 0)
                    log.Error($"Обнаружены записи для наших доменов: {string.Join(",", foundOurDamains)}");
                else
                    log.Success("Записи для наших доменов не обнаружены.");

                log.Info($"Чтение hosts завершено. Проанализировано строк: {lines.Length}");
            }
            catch (Exception ex)
            {
                log.Error($"Ошибка при чтении файла hosts: {ex.Message}");
            }
        }
    }
}
