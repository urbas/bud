using System;
using System.IO;
using Bud.Plugins.CSharp;
using Bud.Plugins.BuildLoading;

namespace Bud {
  public static class BudCli {
    public static void Main(string[] args) {
      EvaluationContext buildConfiguration = BuildLoading.Load(Directory.GetCurrentDirectory());
      buildConfiguration.Evaluate(CSharpKeys.Build);
    }
  }
}
