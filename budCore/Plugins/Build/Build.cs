using System;
using System.Threading.Tasks;

namespace Bud.Plugins.Build {
  public static class Build {
    public static Task BuildAll(this EvaluationContext context) {
      return context.Evaluate(BuildKeys.Build);
    }

    public static Task CleanAll(this EvaluationContext context) {
      return context.Evaluate(BuildKeys.Clean);
    }
  }
}

