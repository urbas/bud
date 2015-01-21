using Bud.Plugins.Build;
using Bud.Plugins.Deps;

namespace Bud.Plugins.CSharp {
  public class CSharpPlugin : IPlugin {
    public static readonly CSharpPlugin Instance = new CSharpPlugin(true);
    private readonly bool addTestBuildTarget;

    public CSharpPlugin(bool addTestBuildTarget) {
      this.addTestBuildTarget = addTestBuildTarget;
    }

    public Settings ApplyTo(Settings settings, Key project) {
      var mainBuildTarget = settings.Apply(project, new CSharpBuildTargetPlugin(BuildKeys.Main));
      if (addTestBuildTarget) {
        return mainBuildTarget.Apply(
          project,
          new CSharpBuildTargetPlugin(BuildKeys.Test,
                                Dependencies.AddDependency(new InternalDependency(CSharpBuildTargetPlugin.MainBuildTargetKey(project), CSharpBuildTargetPlugin.MainBuildTaskKey(project))),
                                CSharpBuildTargetPlugin.ConvertBuildTargetToDll));
      }
      return mainBuildTarget;
    }
  }
}