using NetFixer.Interfaces;
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
        public static async Task<CommandResult> ExecuteAsync(string command, ILog log) =>
            await ExecuteAsync(command, log, logOutput: true, logError: true);

        public static async Task<CommandResult> ExecuteAsync(string command, ILog log, bool logOutput, bool logError)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var psi = new ProcessStartInfo("cmd.exe", $"/c chcp 65001 > nul && {command}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            var process = new Process { StartInfo = psi };
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(outputTask, errorTask);

            var output = await outputTask;
            var error = await errorTask;

            await process.WaitForExitAsync();

            var result = new CommandResult
            {
                Output = output.Trim(),
                Error = error.Trim(),
                ExitCode = process.ExitCode
            };

            if (logOutput && logError)
                log.Command(command, result.Output, result.Error, result.ExitCode);
            else if (!logOutput)
                log.Command(command, "Reading completed successfully", result.Error, result.ExitCode);
            else
                log.Command(command, result.Output, "", result.ExitCode);

            return result;
        }
    }
}
