using Bud.Plugins.Build;
using Bud.Plugins.Deps;

namespace Bud.Plugins.CSharp {
  public class CSharpPlugin : IPlugin {
    public static readonly CSharpPlugin Instance = new CSharpPlugin();

    private CSharpPlugin() {}

    public Settings ApplyTo(Settings settings, Key project) {
      return settings
        .Apply(project, DependenciesPlugin.Instance)
        .Apply(project, new CSharpBuildPlugin(BuildKeys.Main))
        .Apply(project, new CSharpBuildPlugin(BuildKeys.Test,
          Dependencies.AddDependency(new InternalDependency(CSharp.MainBuildTargetKey(project), CSharp.MainBuildTaskKey(project))))
        );
    }
  }
}