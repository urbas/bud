using Bud.Build;
using Bud.Dependencies;
using Bud.Projects;
using NuGet;

namespace Bud.CSharp {
  public class CSharpInternalDependency : InternalDependency {
    public CSharpInternalDependency(Key dependencyBuildTarget) : base(dependencyBuildTarget, BuildKeys.Build.In(dependencyBuildTarget)) {
    }

    public override IPackage AsPackage(IConfig config) {
      return new CSharpBuildTargetPackage(config, DependencyTarget);
    }
  }
}