using System;

namespace Bud.Plugins.Dependencies
{
  public class ScopeDependency : IDependency
  {
    public readonly Scope Scope;

    public ScopeDependency(Scope scopeOfDependency) {
      this.Scope = scopeOfDependency;
    }

    public override string ToString() {
      return string.Format("Dependency({0})", Scope);
    }
	}
}

