using Bud.Build;
using Bud.Dependencies;
using Bud.Projects;

namespace Bud.CSharp {
  public static class Cs {
    public static Setup Dependency(string packageName, string packageVersion = null, string scope = null) {
      var dependencyProject = ProjectPlugin.ProjectKey(packageName);
      var dependencyBuildTarget = dependencyProject / BuildKeys.Main / CSharpKeys.CSharp;
      return DependenciesSettings.AddDependency(
        new CSharpInternalDependency(dependencyBuildTarget),
        new ExternalDependency(packageName, packageVersion), context => IsMainBuildTargetDefined(context, dependencyBuildTarget));
    }

    public static Setup Exe(params Setup[] setups) {
      return new CSharpBuildTargetPlugin(BuildKeys.Main, setups);
    }

    public static Setup Dll(params Setup[] setups) {
      return new CSharpBuildTargetPlugin(BuildKeys.Main,
                                         CSharpKeys.AssemblyType.Modify(AssemblyType.Library),
                                         setups.Merge());
    }

    public static Setup Test(params Setup[] setups) {
      return new CSharpBuildTargetPlugin(BuildKeys.Test,
                                         CSharpKeys.AssemblyType.Modify(AssemblyType.Library),
                                         setups.Merge(),
                                         AddMainBuildTargetDependency());
    }

    private static Setup AddMainBuildTargetDependency() {
      return settings => {
        var project = BuildTargetUtils.ProjectOf(settings.Scope);
        var mainBuildTarget = project / BuildKeys.Main / CSharpKeys.CSharp;
        return settings.Add(DependenciesSettings.AddDependency(new CSharpInternalDependency(mainBuildTarget), config => IsMainBuildTargetDefined(config, mainBuildTarget)));
      };
    }

    private static bool IsMainBuildTargetDefined(IConfig context, Key dependencyBuildTargetKey) {
      return context.IsConfigDefined(dependencyBuildTargetKey / CSharpKeys.OutputAssemblyFile);
    }
  }
}