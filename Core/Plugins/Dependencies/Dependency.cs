using System;

namespace Bud.Plugins.Dependencies
{
  public class Dependency
  {
    public readonly Scope Scope;

    public Dependency(Scope scopeOfDependency) {
      this.Scope = scopeOfDependency;
    }

    public override string ToString() {
      return string.Format("Dependency({0})", Scope);
    }
	}
}

