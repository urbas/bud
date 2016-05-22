using System;
using System.Diagnostics;

namespace Bud.Cli {
  public static class Exec {
    public static string
      GetOutput(string executablePath,
                Option<string> arguments = default(Option<string>),
                Option<string> workingDir = default(Option<string>)) {
      var process = CreateProcess(executablePath,
                                  arguments,
                                  workingDir);
      process.Start();
      process.WaitForExit();
      return process.StandardOutput.ReadToEnd();
    }

    /// <summary>
    ///   Runs the command, suppresses all its output, and throws an exception
    ///   if it returns a no-zero error code.
    /// </summary>
    public static Process
      RunCheckedQuietly(string executablePath,
                        Option<string> arguments = default(Option<string>),
                        Option<string> workingDir = default(Option<string>)) {
      var process = CreateProcess(executablePath, arguments, workingDir);
      // NOTE: If we don't read the output to end sometimes processes get stuck.
      process.OutputDataReceived += (s, a) => {};
      process.Start();
      process.BeginOutputReadLine();
      process.StandardError.ReadToEnd();
      process.WaitForExit();
      if (process.ExitCode != 0) {
        throw new Exception($"Command '{executablePath}' with arguments '{arguments}' at working dir '{workingDir}' failed with error code {process.ExitCode}.");
      }
      return process;
    }

    public static int 
      Run(string executablePath,
          Option<string> arguments = default(Option<string>),
          Option<string> workingDir = default(Option<string>)) {
      var process = CreateProcess(executablePath, arguments, workingDir);
      process.OutputDataReceived += ProcessOnOutputDataReceived;
      process.Start();
      process.BeginOutputReadLine();
      ReadErrorOutput(process);
      process.WaitForExit();
      return process.ExitCode;
    }

    private static Process
      CreateProcess(string executablePath,
                    Option<string> arguments = default(Option<string>),
                    Option<string> workingDir = default(Option<string>)) {
      var process = new Process();
      var argumentsString = arguments.GetOrElse(string.Empty);
      process.StartInfo = new ProcessStartInfo(executablePath, argumentsString) {
        CreateNoWindow = true,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
      };
      if (workingDir.HasValue) {
        process.StartInfo.WorkingDirectory = workingDir.Value;
      }
      return process;
    }

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