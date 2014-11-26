using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Bud.Cli {
  public class ProcessBuilder {
    private readonly StringBuilder arguments = new StringBuilder();

    public ProcessBuilder(string executablePath) {
      ExecutablePath = executablePath;
    }

    public string ExecutablePath { get; private set; }

    public string Arguments { get { return arguments.ToString(); } }

    public ProcessBuilder AddArgument(string argument) {
      if (arguments.Length > 0) {
        arguments.Append(' ');
      }
      arguments.Append('"').Append(argument).Append('"');
      return this;
    }

    public ProcessBuilder AddArguments(params string[] arguments) {
      return AddArguments((IEnumerable<string>)arguments);
    }

    public ProcessBuilder AddArguments(IEnumerable<string> arguments) {
      foreach (var argument in arguments) {
        AddArgument(argument);
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

    public int Execute(TextWriter output, TextWriter errorOutput) {
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

