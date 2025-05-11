using NetFixer.Interfaces;

namespace NetFixer.Logging
{
    public class CombinedLogHandler : ILog
    {
        private readonly ILog _consoleLog;
        private readonly ILog _fileLog;

        public CombinedLogHandler(ILog consoleLog, ILog fileLog)
        {
            _consoleLog = consoleLog;
            _fileLog = fileLog;
        }

        public void Info(string message)
        {
            _consoleLog.Info(message);
            _fileLog.Info(message);
        }

        public void Success(string message)
        {
            _consoleLog.Success(message);
            _fileLog.Success(message);
        }

        public void Error(string message)
        {
            _consoleLog.Error(message);
            _fileLog.Error(message);
        }

        public void Command(string command, string output, string error, int exitCode)
        {
            _consoleLog.Command(command, output, error, exitCode);
            _fileLog.Command(command, output, error, exitCode);
        }

        public void SaveToFile()
        {
            if (_fileLog is FileLogHandler fl)
                fl.SaveToFile();
        }
    }
}
