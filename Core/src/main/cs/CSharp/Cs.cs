using Bud.Build;
using Bud.Dependencies;
using Bud.Projects;

namespace Bud.CSharp {
  public static class Cs {
    public static Setup Dependency(string packageName, string packageVersion = null, string scope = null) {
      var dependencyProjectKey = ProjectsSettings.ProjectKey(packageName);
      var dependencyBuildTargetKey = CSharpBuildTargetPlugin.MainBuildTargetKey(dependencyProjectKey);
      return DependenciesSettings.AddDependency(
        new InternalDependency(dependencyBuildTargetKey, CSharpBuildTargetPlugin.MainBuildTaskKey(dependencyProjectKey)),
        new ExternalDependency(packageName, packageVersion),
        shouldUseInternalDependency: context => IsMainBuildTargetDefined(context, dependencyBuildTargetKey));
    }

    public static Setup Exe(params Setup[] setups) {
      return CSharpBuildTargetPlugin.Init(BuildKeys.Main, setups);
    }

    public static Setup Dll(params Setup[] setups) {
      return CSharpBuildTargetPlugin.Init(BuildKeys.Main, CSharpKeys.AssemblyType.Modify(AssemblyType.Library), setups.ToPlugin());
    }

    public static Setup Test(params Setup[] setups) {
      return CSharpBuildTargetPlugin.Init(BuildKeys.Test, CSharpKeys.AssemblyType.Modify(AssemblyType.Library), setups.ToPlugin(), AddMainBuildTargetAsDependency);
    }

    private static Settings AddMainBuildTargetAsDependency(Settings settings) {
      var buildTarget = settings.Scope;
      var mainBuildTarget = CSharpBuildTargetPlugin.FindSiblingMainBuildTarget(buildTarget);
      var mainBuildTask = CSharpBuildTargetPlugin.FindSiblingMainBuildTask(buildTarget);
      return settings.AddDependency(new InternalDependency(mainBuildTarget, mainBuildTask), config => IsMainBuildTargetDefined(config, mainBuildTarget));
    }

    private static bool IsMainBuildTargetDefined(IConfig context, Key dependencyBuildTargetKey) {
      return context.IsConfigDefined(CSharpKeys.OutputAssemblyFile.In(dependencyBuildTargetKey));
    }
  }
}