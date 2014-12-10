using System;
using System.Linq;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using Bud.Plugins.CSharp;
using Bud.Plugins.Build;
using System.IO;
using System.Collections.Generic;
using Bud;
using System.Threading.Tasks;
using System.Reflection;
using Bud.Commander;

namespace Bud.Commander
{
  public static class CommandEvaluator {
    public static string Evaluate(Settings settings, string command) {
      if (BuildKeys.Build.ToString().Equals(command)) {
        EvaluationContext.FromSettings(settings).BuildAll().Wait();
      }
      else {
        EvaluationContext.FromSettings(settings).CleanAll().Wait();
      }
      return string.Empty;
    }
  }
}

