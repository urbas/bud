using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Bud.Cli;
using Bud.IO;

namespace Bud.CSharp {
  public class CSharpExeRunnerPlugin : Plugin {
    public static readonly CSharpExeRunnerPlugin Instance = new CSharpExeRunnerPlugin();
    public static readonly TaskKey<ExecutionResult> Run = Key.Define("run", "Runs executable build targets.");
    public static readonly ConfigKey<ImmutableList<TaskKey<ExecutionResult>>> RunList = Key.Define("runList", "All defined run keys.");
    public static readonly TaskKey<IEnumerable<ExecutionResult>> RunGlobal = Key.Define("run", "Runs all executable build targets defined in this build.");

    public override Settings Setup(Settings settings) {
      var buildTarget = settings.Scope;
      return settings.Add(Run.Init(RunImpl))
                     .AddGlobally(RunList.Init(ImmutableList<TaskKey<ExecutionResult>>.Empty),
                                  RunList.Modify((config, previousRunList) => previousRunList.Add(buildTarget / Run)),
                                  RunGlobal.Init(RunGlobalImpl));
    }

    private static async Task<IEnumerable<ExecutionResult>> RunGlobalImpl(IContext context) {
      var runList = context.Evaluate(RunList);
      var executionResults = new List<ExecutionResult>();
      foreach (var taskKey in runList) {
        var executionResult = await context.Evaluate(taskKey);
        executionResults.Add(executionResult);
      }
      return executionResults;
    }

    private static async Task<ExecutionResult> RunImpl(IContext context, Key buildTarget) {
      await context.Evaluate(buildTarget / Cs.Dist);
      var distributionPath = context.GetDistDir(buildTarget);
      var executable = Path.Combine(distributionPath, Path.GetFileName(context.GetCSharpOutputAssemblyFile(buildTarget)));
      context.Logger.Info(String.Format("Executing {0}...", executable));
      return ExecuteAssembly(executable);
    }

    private static ExecutionResult ExecuteAssembly(string executable) {
      var recordedOutput = new RecordingTextWriter(Console.Out);
      var recordedError = new RecordingTextWriter(Console.Error);
      var exitCode = ProcessBuilder.Executable(executable).Start(recordedOutput, recordedError);
      return new ExecutionResult(exitCode, recordedOutput.ToString(), recordedError.ToString());
    }
  }
}