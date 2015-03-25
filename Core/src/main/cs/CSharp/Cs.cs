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
        new ExternalDependency(packageName, packageVersion), context => CSharpBuildTargetPlugin.IsMainBuildTargetDefined(context, dependencyBuildTarget));
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
      return new NUnitTestTargetPlugin(BuildKeys.Test, setups.Merge());
    }
  }
}