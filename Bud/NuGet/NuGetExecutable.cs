using System;
using System.Diagnostics;

namespace Bud.NuGet {
  public class NuGetExecutable {
    public static readonly NuGetExecutable Instance = new NuGetExecutable();

    public virtual bool Run(string args) => ExecuteNuGet(args) == 0;

    public static int ExecuteNuGet(string arguments) {
      var process = new Process();
      process.StartInfo = new ProcessStartInfo("nuget", arguments) {
        CreateNoWindow = true,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
      };
      process.OutputDataReceived += ProcessOnOutputDataReceived;
      process.Start();
      process.BeginOutputReadLine();
      ReadErrorOutput(process);
      process.WaitForExit();
      return process.ExitCode;
    }

    private static void ReadErrorOutput(Process process) {
      var errorOutput = process.StandardError.ReadToEnd();
      if (!string.IsNullOrEmpty(errorOutput)) {
        Console.Write(errorOutput);
      }
    }

    private static void ProcessOnOutputDataReceived(object sender,
                                                    DataReceivedEventArgs outputLine)
      => Console.WriteLine(outputLine.Data);
  }
}