using System;
using System.Threading.Tasks;

namespace Bud.Plugins.Dependencies
{
  public class ScopeDependency
  {
    public readonly Key Scope;

    public ScopeDependency(Key scopeOfDependency) {
      this.Scope = scopeOfDependency;
    }

    public Task<ResolvedScopeDependency> Resolve(EvaluationContext context) {
      return context.Evaluate(DependenciesKeys.ResolveScopeDependency.In(Scope));
    }

    public override string ToString() {
      return string.Format("Dependency({0})", Scope);
    }
	}
}

