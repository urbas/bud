using System;
using System.Diagnostics;

namespace Bud.Cli {
  public static class Exec {
    /// <summary>
    ///   Runs the command, suppresses all its output, and throws an exception
    ///   if it returns a no-zero error code.
    /// </summary>
    public static void RunCheckedQuietly(string executablePath, string arguments) {
      var process = CreateWindowlessProcess(executablePath, arguments);
      process.Start();
      process.WaitForExit();
      if (process.ExitCode != 0) {
        throw new Exception($"The command '{executablePath}' with arguments '{arguments}' returned an error code of {process.ExitCode}.");
      }
    }

    public static int Run(string executablePath, string arguments) {
      var process = CreateWindowlessProcess(executablePath, arguments);
      process.OutputDataReceived += ProcessOnOutputDataReceived;
      process.Start();
      process.BeginOutputReadLine();
      ReadErrorOutput(process);
      process.WaitForExit();
      return process.ExitCode;
    }

    private static Process CreateWindowlessProcess(string executablePath, string arguments)
      => new Process {
        StartInfo = new ProcessStartInfo(executablePath, arguments) {
          CreateNoWindow = true,
          UseShellExecute = false,
          RedirectStandardOutput = true,
          RedirectStandardError = true
        }
      };

    private static void ReadErrorOutput(Process process) {
      var errorOutput = process.StandardError.ReadToEnd();
      if (!string.IsNullOrEmpty(errorOutput)) {
        Console.Write(errorOutput);
      }
    }

    private static void ProcessOnOutputDataReceived(object sender,
                                                    DataReceivedEventArgs outputLine) {
      if (outputLine.Data != null) {
        Console.WriteLine(outputLine.Data);
      }
    }
  }
}