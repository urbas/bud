using System;

namespace Bud.Plugins.Dependencies
{
  public class Dependency
  {
    public readonly Scope Scope;

    public Dependency(Scope scopeOfDependency) {
      this.Scope = scopeOfDependency;
    }
	}

}

