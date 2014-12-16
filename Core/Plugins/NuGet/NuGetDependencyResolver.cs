using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using System.IO;
using Bud.Commander;
using System.Threading.Tasks;
using Bud.Plugins.Dependencies;

namespace Bud.Plugins.NuGet {
  public class NuGetDependencyResolver {
    public Task<IResolvedDependency> Resolve(EvaluationContext context, NuGetDependency nuGetDependency) {
      // TODO: Call NuGet here. Maybe create a concurrent queue, put the dependency onto the queue (if not added already), start a resolver thread, and return an awaitable.
      return Task.FromResult((IResolvedDependency)new NuGetResolvedDependency(nuGetDependency));
    }
  }
}

