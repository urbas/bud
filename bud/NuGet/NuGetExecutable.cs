using Bud.Cli;

namespace Bud.NuGet {
  public class NuGetExecutable {
    public static readonly NuGetExecutable Instance = new NuGetExecutable();

    public virtual bool Run(string args) => Exec.Run("nuget", args) == 0;
  }
}