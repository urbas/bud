using Bud.Build;
using Bud.Dependencies;
using Bud.Projects;

namespace Bud.CSharp {
  public static class Cs {
    public static Setup Dependency(string packageName, string packageVersion = null, string scope = null) {
      var dependencyProject = ProjectsSettings.ProjectKey(packageName);
      var dependencyBuildTargetKey = BuildUtils.BuildTarget(dependencyProject, BuildKeys.Main, CSharpKeys.CSharp);
      return DependenciesSettings.AddDependency(
        new InternalDependency(dependencyBuildTargetKey, BuildUtils.BuildTaskKey(dependencyProject, BuildKeys.Main, CSharpKeys.CSharp)),
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
      return CSharpBuildTargetPlugin.Init(BuildKeys.Test, CSharpKeys.AssemblyType.Modify(AssemblyType.Library), setups.ToPlugin(), AddMainBuildTargetAsDependency());
    }

    private static Setup AddMainBuildTargetAsDependency() {
      return settings => {
        var project = settings.Scope.Parent.Parent;
        var mainBuildTarget = BuildUtils.BuildTarget(project, BuildKeys.Main, CSharpKeys.CSharp);
        var mainBuildTask = BuildUtils.BuildTaskKey(project, BuildKeys.Main, CSharpKeys.CSharp);
        return settings.Do(DependenciesSettings.AddDependency(new InternalDependency(mainBuildTarget, mainBuildTask), config => IsMainBuildTargetDefined(config, mainBuildTarget)));
      };
    }

    private static bool IsMainBuildTargetDefined(IConfig context, Key dependencyBuildTargetKey) {
      return context.IsConfigDefined(CSharpKeys.OutputAssemblyFile.In(dependencyBuildTargetKey));
    }
  }
}