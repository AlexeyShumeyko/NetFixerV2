using NetFixer.Interfaces;

namespace NetFixer.Plugins.Connection
{
    public class HostsFileCheckPlugin : INetFixPlugin
    {
        public string Name => "Проверка содержимого hosts";

        public async Task ExecuteAsync(ILog log, CancellationToken cancellationToken)
        {
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

                if (lines.Length == 0)
                {
                    log.Info("Файл hosts пуст.");

                    return;
                }

                foreach (var line in lines)
                {
                    log.Info("  " + line);
                }

                log.Success("Чтение hosts завершено.");
            }
            catch (Exception ex)
            {
                log.Error($"Ошибка при чтении файла hosts: {ex.Message}");
            }
        }
    }
}
