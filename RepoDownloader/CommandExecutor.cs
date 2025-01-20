using System.Diagnostics;

namespace RepoDownloader;

internal static class CommandExecutor
{
    public static (int ExitCode, string Output, string Error) ExecuteCommand(string command)
    {
        int exitCode = -1; // Initialize to a default value
        string output;
        string error;

        try
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd", $"/c {command}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (Process process = new Process() { StartInfo = processStartInfo })
            {
                process.Start();

                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                exitCode = process.ExitCode; // Store the exit code

                // Log command, output, error, and exit code
                LogCommandDetails(command, output, error, exitCode);
            }

            // Return after the using block, ensuring process disposal
            return (exitCode, output, error);
        }
        catch (Exception ex)
        {
            // Log exceptions
            Console.WriteLine($"Exception in ExecuteCommand: {ex}");
            return (exitCode, string.Empty, ex.Message);
        }
    }

    private static void LogCommandDetails(string command, string output, string error, int exitCode)
    {
        Console.WriteLine($"Command: {command}");
        Console.WriteLine($"Exit Code: {exitCode}");
        Console.WriteLine($"Output:{Environment.NewLine}{output}");
        Console.WriteLine($"Error:{Environment.NewLine}{error}");
    }
}

