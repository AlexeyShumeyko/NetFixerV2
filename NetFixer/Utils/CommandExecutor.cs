using NetFixer.Interfaces;
using NetFixer.Logging;
using System.Diagnostics;
using System.Text;

namespace NetFixer.Utils
{
    public class CommandResult
    {
        public string Output { get; init; } = "";
        public string Error { get; init; } = "";
        public int ExitCode { get; init; }
        public bool IsSuccess => ExitCode == 0;
    }

    public static class CommandExecutor
    {
        public static async Task<CommandResult> ExecuteAsync(string command, ILog log)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            psi.StandardOutputEncoding = Encoding.GetEncoding("cp866");
            psi.StandardErrorEncoding = Encoding.GetEncoding("cp866");

            var process = new Process { StartInfo = psi };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            var result = new CommandResult
            {
                Output = output.Trim(),
                Error = error.Trim(),
                ExitCode = process.ExitCode
            };

            log.Command(command, result.Output, result.Error, result.ExitCode);
            return result;
        }
    }
}
