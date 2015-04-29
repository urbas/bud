using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Bud.IO;

namespace Bud.Cli {
  public class ProcessBuilder {
    private readonly StringBuilder arguments = new StringBuilder();

    public static ProcessBuilder Executable(string executablePath) {
      return new ProcessBuilder(executablePath);
    }

    public static int Execute(string executable, params string[] commandLineArgs) {
      return Executable(executable).AddArguments(commandLineArgs).Start();
    }

    public ProcessBuilder(string executablePath) {
      ExecutablePath = executablePath;
    }

    public string ExecutablePath { get; }

    public string Arguments => arguments.ToString();

    public ProcessBuilder AddArgument(string argument) => AddParamArgument(argument, ImmutableList<string>.Empty);

    public ProcessBuilder AddParamArgument(string argumentHead, params string[] argumentParams) => AddParamArgument(argumentHead, (IEnumerable<string>) argumentParams);

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

    public ProcessBuilder AddArguments(params string[] arguments) => AddArguments((IEnumerable<string>) arguments);

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

    public int Start() => Start(Console.Out, Console.Error);

    public int Start(TextWriter output, TextWriter errorOutput) {
      using (var process = ToProcess()) {
        process.Start();
        Task asyncOutputPipe = TextWriterUtils.PipeReadAsyncWriteSync(process.StandardOutput, output);
        Task asyncErrorOutputPipe = TextWriterUtils.PipeReadAsyncWriteSync(process.StandardError, errorOutput);
        Task.WaitAll(asyncOutputPipe, asyncErrorOutputPipe);
        process.WaitForExit();
        return process.ExitCode;
      }
    }
  }
}