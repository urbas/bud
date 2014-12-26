using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Bud.Plugins.Build;
using Bud.Plugins.BuildLoading;

namespace Bud.Commander {
  public static class BuildCommanderUtils {
    public static string Evaluate(this IBuildCommander budCommander, Scope scope) {
      var evaluation = budCommander.Evaluate(scope.ToString());
      evaluation.Result.Wait();
      return evaluation.Result.Result;
    }
  }
}
