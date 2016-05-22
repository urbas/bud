using Bud.Cli;

namespace Bud.NuGet {
  public class NuGetExecutable {
    public static readonly NuGetExecutable Instance = new NuGetExecutable();

    public virtual bool Run(string args) => BatchExec.Run("nuget", args) == 0;
  }
}