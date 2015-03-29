using Bud.Build;
using Bud.Dependencies;
using Bud.Projects;

namespace Bud.CSharp {
  public static class Cs {
    public static Setup Dependency(string packageName, string packageVersion = null, string scope = null) {
      var dependencyProject = Project.ProjectKey(packageName);
      var dependencyBuildTarget = dependencyProject / BuildKeys.Main / CSharpKeys.CSharp;
      return DependenciesSettings.AddDependency(
        new CSharpInternalDependency(dependencyBuildTarget),
        new ExternalDependency(packageName, packageVersion), context => CsBuild.IsMainBuildTargetDefined(context, dependencyBuildTarget));
    }

    public static Setup Exe(params Setup[] setups) {
      return new CsBuild(BuildKeys.Main, new CSharpExeRunnerPlugin(), setups.Merge());
    }

    public static Setup Dll(params Setup[] setups) {
      return new CsBuild(BuildKeys.Main,
                         CSharpKeys.AssemblyType.Modify(AssemblyType.Library),
                         setups.Merge());
    }

    public static Setup Test(params Setup[] setups) {
      return new NUnitTestTargetPlugin(BuildKeys.Test, setups.Merge());
    }
  }
}