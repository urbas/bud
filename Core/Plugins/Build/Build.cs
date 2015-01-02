using System;
using System.Threading.Tasks;

namespace Bud.Plugins.Build {
  public static class Build {
    public static Task BuildScope(this EvaluationContext context, Key scope) {
      return context.Evaluate(BuildKeys.Build.In(scope));
    }

    public static Task BuildAll(this EvaluationContext context) {
      return context.Evaluate(BuildKeys.Build);
    }

    public static Task CleanAll(this EvaluationContext context) {
      return context.Evaluate(BuildKeys.Clean);
    }
  }
}

