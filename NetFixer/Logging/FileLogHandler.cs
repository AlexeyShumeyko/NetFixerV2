using NetFixer.Core;
using NetFixer.Interfaces;
using System.Text;

namespace NetFixer.Logging
{
    public class FileLogHandler : ILog
    {
        private readonly StringBuilder _fileLog = new();
        private readonly string _filePath;

        public FileLogHandler()
        {
            var logDir = AppContext.BaseDirectory;
            var fileName = $"DiagnosticLog_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.txt";

            _filePath = Path.Combine(logDir, fileName);
            Info($"[Init] Лог-файл: {_filePath}");
        }

        public void Info(string message) => Write("[INFO]", message);
        public void Success(string message) 
        {
            Write("[SUCCESS]", message);
            _fileLog.AppendLine(new string('-', 100));
        }
        public void Error(string message) => Write("[ERROR]", message);

        public void Command(string command, string output, string error, int exitCode)
        {
            var block = new StringBuilder();

            block.AppendLine($"{DateTime.Now:HH:mm:ss} [COMMAND] {command}");
            block.AppendLine($"{DateTime.Now:HH:mm:ss} [EXIT CODE] {exitCode}");

            if (!string.IsNullOrWhiteSpace(output))
            {
                var lines = output.Split(
                    new[] { "\r\n", "\n" },
                    StringSplitOptions.RemoveEmptyEntries
                );

                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        block.AppendLine($"{DateTime.Now:HH:mm:ss} [STDOUT] {line.Trim()}");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(error))
                block.AppendLine($"{DateTime.Now:HH:mm:ss} [STDERR] {error}");

            _fileLog.AppendLine(block.ToString());
        }

        private void Write(string type, string message)
        {
            _fileLog.AppendLine($"{DateTime.Now:HH:mm:ss} {type} {message}");
        }

        public void SaveToFile()
        {
            File.WriteAllText(_filePath, _fileLog.ToString());
        }

        public string FilePath => _filePath;
    }
}
