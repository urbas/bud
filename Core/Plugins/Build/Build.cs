using System;
using System.Threading.Tasks;

namespace Bud.Plugins.Build {
  public static class Build {
    public static Task BuildTarget(this IContext context, Key target) {
      return context.Evaluate(BuildKeys.Build.In(target));
    }

    public static Task BuildAll(this IContext context) {
      return context.Evaluate(BuildKeys.Build);
    }

    public static Task CleanAll(this IContext context) {
      return context.Evaluate(BuildKeys.Clean);
    }
  }
}

