using System;

namespace Bud.Plugins.Dependencies
{
  public class ScopeDependency : IDependency
  {
    public readonly Scope Scope;

    public ScopeDependency(Scope scopeOfDependency) {
      this.Scope = scopeOfDependency;
    }

    public System.Threading.Tasks.Task<IResolvedDependency> Resolve(EvaluationContext context) {
      return context.Evaluate(DependenciesKeys.ResolveScopeDependency.In(Scope));
    }

    public override string ToString() {
      return string.Format("Dependency({0})", Scope);
    }
	}
}

