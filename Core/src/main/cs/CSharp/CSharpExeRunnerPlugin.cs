using System;
using System.IO;
using System.Threading.Tasks;
using Bud.Cli;
using Bud.Util;

namespace Bud.CSharp {
  public class CSharpExeRunnerPlugin : Plugin {
    public static readonly TaskKey Run = Key.Define("run", "Runs executable build targets.");

    public override Settings Setup(Settings settings) {
      var buildTarget = settings.Scope;
      return settings.Add(Run.Init(RunImpl))
                     .AddGlobally(Run.Init(TaskUtils.NoOpTask),
                                  Run.DependsOn(buildTarget / Run));
    }

    private async Task RunImpl(IContext context, Key buildTarget) {
      await context.Evaluate(buildTarget / CSharpKeys.Dist);
      var distributionPath = context.GetDistDir(buildTarget);
      var executable = Path.Combine(distributionPath, Path.GetFileName(context.GetCSharpOutputAssemblyFile(buildTarget)));
      context.Logger.Info(string.Format("Executing {0}...", executable));
      ProcessBuilder.Executable(executable).Start(Console.Out, Console.Error);
    }
  }
}