using Bud.Build;
using Bud.Dependencies;
using NuGet;

namespace Bud.CSharp {
  public class CSharpInternalDependency : InternalDependency {
    public CSharpInternalDependency(Key dependencyBuildTarget) : base(dependencyBuildTarget, dependencyBuildTarget / BuildKeys.Build) {}

    public override IPackage AsPackage(IConfig config) {
      return new CSharpBuildTargetPackage(config, DependencyTarget);
    }
  }
}