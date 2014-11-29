using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Bud.Cli {
  public class ProcessBuilder {
    private readonly StringBuilder arguments = new StringBuilder();

    public static ProcessBuilder Executable(string executablePath) {
      return new ProcessBuilder(executablePath);
    }

    public ProcessBuilder(string executablePath) {
      ExecutablePath = executablePath;
    }

    public string ExecutablePath { get; private set; }

    public string Arguments { get { return arguments.ToString(); } }

    public ProcessBuilder WithArgument(string argument) {
      if (arguments.Length > 0) {
        arguments.Append(' ');
      }
      arguments.Append('"').Append(argument).Append('"');
      return this;
    }

    public ProcessBuilder WithArguments(params string[] arguments) {
      return WithArguments((IEnumerable<string>)arguments);
    }

    public ProcessBuilder WithArguments(IEnumerable<string> arguments) {
      foreach (var argument in arguments) {
        WithArgument(argument);
      }
      return this;
    }

    public Process ToProcess() {
      var process = new Process();
      process.StartInfo.FileName = ExecutablePath;
      process.StartInfo.Arguments = Arguments;
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      return process;
    }

    public int Start(TextWriter output, TextWriter errorOutput) {
      using (var process = ToProcess()) {
        process.Start();
        // TODO(urbas): Pipe the outputs using async operations.
        output.WriteLine(process.StandardOutput.ReadToEnd());
        errorOutput.WriteLine(process.StandardError.ReadToEnd());
        process.WaitForExit();
        return process.ExitCode;
      }
    }
  }
}

