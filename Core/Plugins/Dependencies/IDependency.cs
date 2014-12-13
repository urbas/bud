using System;
using System.Threading.Tasks;

namespace Bud.Plugins.Dependencies
{
  public interface IDependency {
    Task<IResolvedDependency> Resolve(EvaluationContext context);
  }
  
}
