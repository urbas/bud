using System;
using System.Threading.Tasks;

namespace Bud.Plugins.Build {
  public static class Build {
    public static Task BuildTarget(this IEvaluationContext context, Key target) {
      return context.Evaluate(BuildKeys.Build.In(target));
    }

    public static Task BuildAll(this IEvaluationContext context) {
      return context.Evaluate(BuildKeys.Build);
    }

    public static Task CleanAll(this IEvaluationContext context) {
      return context.Evaluate(BuildKeys.Clean);
    }
  }
}

