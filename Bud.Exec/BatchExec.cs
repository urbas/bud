using System;
using System.Diagnostics;
using System.IO;

namespace Bud {
  public static class BatchExec {
    /// <summary>
    ///   Runs the executable at path '<paramref name="executablePath" />'
    ///   with the given arguments '<paramref name="arguments" />' in the
    ///   given working directory '<paramref name="workingDir" />',
    ///   waits for the executable to finish,
    ///   and returns the exit code of the process.
    /// </summary>
    /// <param name="executablePath">the path of the executable to run.</param>
    /// <param name="arguments">the arguments to be passed to the executable.</param>
    /// <param name="workingDir">
    ///   the working directory in which to run. If omitted, the executable will run in the current
    ///   working directory.
    /// </param>
    /// <returns>the exit code of the process.</returns>
    /// <remarks>
    ///   The standard output and standard error of the process are redirected to standard output and
    ///   standard error of this process.
    ///   <para>Note that this function does not redirect standard input.</para>
    /// </remarks>
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

    /// <summary>
    ///   Runs the command in batch mode (without any input), suppresses all output, and throws an exception
    ///   if it returns a no-zero error code.
    /// </summary>
    /// <returns>the <see cref="Process" /> object used to run the executable.</returns>
    public static Process
      RunQuietlyThrow(string executablePath,
                      Option<string> arguments = default(Option<string>),
                      Option<string> workingDir = default(Option<string>)) {
      var process = CreateProcess(executablePath, arguments, workingDir);
      // NOTE: If we don't read the output to end sometimes processes get stuck.
      process.OutputDataReceived += (s, a) => {};
      process.Start();
      process.BeginOutputReadLine();
      process.StandardError.ReadToEnd();
      process.WaitForExit();
      AssertProcessSucceeded(executablePath, arguments, workingDir, process);
      return process;
    }

    /// <summary>
    ///   Runs the executable at <paramref name="executablePath" /> in batch mode (not passing any input
    ///   to the process), captures all its standard output (ignoring standard error) into a string,
    ///   waits for the process to finish, and returns the captured output as a string upon completion.
    /// </summary>
    /// <param name="executablePath">the path of the executable to run.</param>
    /// <param name="arguments">the arguments to be passed to the executable.</param>
    /// <param name="workingDir">
    ///   the working directory in which to run. If omitted, the executable will run in the current
    ///   working directory.
    /// </param>
    /// <returns>the captured output of the executed process.</returns>
    /// <exception cref="Exception">thrown if the output exits with non-zero error code. The message
    /// of the exception will contain the error output.</exception>
    public static string
      GetOutputOrThrow(string executablePath,
                       Option<string> arguments = default(Option<string>),
                       Option<string> workingDir = default(Option<string>)) {
      var process = CreateProcess(executablePath, arguments, workingDir);
      var stringWriter = new StringWriter();
      process.ErrorDataReceived += (s, a) => {stringWriter.Write(a.Data);};
      process.Start();
      var output = process.StandardOutput.ReadToEnd();
      process.WaitForExit();
      AssertProcessSucceeded(executablePath, arguments, workingDir, process);
      return output;
    }

    private static void AssertProcessSucceeded(string executablePath, Option<string> arguments, Option<string> workingDir, Process process) {
      if (process.ExitCode != 0) {
        throw new Exception($"Command '{executablePath}' with arguments '{arguments}' at working dir '{workingDir}' failed with error code {process.ExitCode}.");
      }
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
        Console.Error.Write(errorOutput);
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