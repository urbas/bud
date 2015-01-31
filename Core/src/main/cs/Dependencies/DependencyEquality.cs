using System.Collections.Generic;

namespace Bud.Dependencies {
  internal class DependencyEquality : IEqualityComparer<IDependency> {
    private readonly IConfig config;

    public DependencyEquality(IConfig config) {
      this.config = config;
    }

    public bool Equals(IDependency dependencyA, IDependency dependencyB) {
      return dependencyA.AsPackage(config).Id.Equals(dependencyA.AsPackage(config).Id);
    }

    public int GetHashCode(IDependency dependency) {
      return dependency.AsPackage(config).Id.GetHashCode();
    }
  }
}