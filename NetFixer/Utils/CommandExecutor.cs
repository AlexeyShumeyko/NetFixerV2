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
        public static async Task<CommandResult> ExecuteAsync(string command, ILog log, CancellationToken token = default) =>
            await ExecuteAsync(command, log, logOutput: true, logError: true, token);

        public static async Task<CommandResult> ExecuteAsync(string command, ILog log, bool logOutput, bool logError, CancellationToken token = default)
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

            using var process = new Process { StartInfo = psi };
            process.Start();

            try
            {
                var outputTask = process.StandardOutput.ReadToEndAsync(token);
                var errorTask = process.StandardError.ReadToEndAsync(token);

                await Task.WhenAll(outputTask, errorTask).WaitAsync(token);

                var output = await outputTask;
                var error = await errorTask;

                await process.WaitForExitAsync(token);

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
            catch (OperationCanceledException)
            {
                try { process.Kill(true); } catch { }

                try
                {
                    using var killer = new Process();
                    killer.StartInfo = new ProcessStartInfo("taskkill", $"/F /T /PID {process.Id}")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    killer.Start();
                    killer.WaitForExit(2000);
                }
                catch { }

                return new CommandResult
                {
                    Output = "",
                    Error = "Command timed out",
                    ExitCode = -1
                };
            }
        }
    }
}