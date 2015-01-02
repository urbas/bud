using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Bud.Plugins.Build;

namespace Bud.Plugins.Dependencies
{
  public class ResolvedScopeDependency
	{
    public ResolvedScopeDependency(Key scope) {
      Dependency = scope;
    }

    public Key Dependency { get; private set; }
	}
}

