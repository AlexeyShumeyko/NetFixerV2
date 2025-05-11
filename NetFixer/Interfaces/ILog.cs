namespace NetFixer.Interfaces
{
    public interface ILog
    {
        void Info(string message);
        void Success(string message);
        void Error(string message);
        void Command(string command, string output, string error, int exitCode);
    }
}
