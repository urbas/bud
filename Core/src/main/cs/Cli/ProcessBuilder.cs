using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Text;

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

    public string Arguments {
      get { return arguments.ToString(); }
    }

    public ProcessBuilder AddArgument(string argument) {
      return AddParamArgument(argument, ImmutableList<string>.Empty);
    }

    public ProcessBuilder AddParamArgument(string argumentHead, params string[] argumentParams) {
      return AddParamArgument(argumentHead, (IEnumerable<string>) argumentParams);
    }

    public ProcessBuilder AddParamArgument(string argumentHead, IEnumerable<string> argumentParams) {
      if (arguments.Length > 0) {
        arguments.Append(' ');
      }
      arguments.Append('"').Append(argumentHead);
      var argumentEnumerator = argumentParams.GetEnumerator();
      if (argumentEnumerator.MoveNext()) {
        arguments.Append(argumentEnumerator.Current);
        while (argumentEnumerator.MoveNext()) {
          arguments.Append(",").Append(argumentEnumerator.Current);
        }
      }
      arguments.Append('"');
      return this;
    }

    public ProcessBuilder AddArguments(params string[] arguments) {
      return AddArguments((IEnumerable<string>) arguments);
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
      process.StartInfo.CreateNoWindow = true;
      return process;
    }

    public int Start(TextWriter output, TextWriter errorOutput) {
      using (var process = ToProcess()) {
        process.Start();
        output.WriteLine(process.StandardOutput.ReadToEnd());
        errorOutput.WriteLine(process.StandardError.ReadToEnd());
        process.WaitForExit();
        return process.ExitCode;
      }
    }
  }
}