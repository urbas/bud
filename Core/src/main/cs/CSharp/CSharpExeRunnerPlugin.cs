using System.Threading.Tasks;
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

    private Task RunImpl(IContext context, Key buildTarget) {
      context.Logger.Info(string.Format("Executing {0}", context.GetCSharpOutputAssemblyFile(buildTarget)));
      return TaskUtils.NullAsyncResult;
    }
  }
}