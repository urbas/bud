using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bud.Plugins.Dependencies
{
  public interface IResolvedDependency {
  }

  public interface IResolvedDependency<TDependency> : IResolvedDependency
	{
    TDependency Dependency { get; }
	}
}
