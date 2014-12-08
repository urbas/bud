using System;

namespace Bud.Plugins.Dependencies
{
	public class DependencySource
	{
    public readonly Scope ScopeOfProvider;

    public DependencySource(Scope scopeOfProvider) {
      this.ScopeOfProvider = scopeOfProvider;
      
    }
	}

}

