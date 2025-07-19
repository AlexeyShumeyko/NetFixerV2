using NetFixer.Interfaces;

namespace NetFixer.Logging
{
    public class CombinedLogHandler : ILog
    {
        private readonly ILog _consoleLog;
        //private readonly ILog _fileLog;
        private readonly ILog _htmlFileLog;

        public CombinedLogHandler(ILog consoleLog, ILog htmlFileLog)
        {
            _consoleLog = consoleLog;
            _htmlFileLog = htmlFileLog;
        }

        public void Info(string message)
        {
            _consoleLog.Info(message);
            _htmlFileLog.Info(message);
        }

        public void Success(string message)
        {
            _consoleLog.Success(message);
            _htmlFileLog.Success(message);
        }

        public void Error(string message)
        {
            _consoleLog.Error(message);
            _htmlFileLog.Error(message);
        }

        public void Command(string command, string output, string error, int exitCode)
        {
            _consoleLog.Command(command, output, error, exitCode);
            _htmlFileLog.Command(command, output, error, exitCode);
        }

        public void Group(string message)
        {
            _consoleLog.Group(message);
            _htmlFileLog.Group(message);
        }

        public void StartPluginGroup(string message)
        {
            _consoleLog.StartPluginGroup(message);
            _htmlFileLog.StartPluginGroup(message);
        }

        public void SubSection(string message)
        {
            _consoleLog.SubSection(message);
            _htmlFileLog.SubSection(message);
        }

        public void SaveToFile()
        {
            //if (_fileLog is FileLogHandler fl)
            //    fl.SaveToFile();

            if (_htmlFileLog is HtmlFileLogHandler html)
                html.SaveToFile();
        }
    }
}
