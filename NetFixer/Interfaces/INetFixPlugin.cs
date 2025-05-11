namespace NetFixer.Interfaces
{
    public interface INetFixPlugin
    {
        string Name { get; }
        Task ExecuteAsync(ILog log, CancellationToken token);
    }
}
