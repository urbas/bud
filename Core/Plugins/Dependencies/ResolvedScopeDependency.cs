using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Bud.Plugins.Build;

namespace Bud.Plugins.Dependencies
{
  public class ResolvedScopeDependency
	{
    public ResolvedScopeDependency(Scope scope) {
      Dependency = scope;
    }

    public Scope Dependency { get; private set; }
	}
}

