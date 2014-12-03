using System;
using System.IO;
using Bud.Plugins.CSharp;

namespace Bud {
  class BudCli {
    public static void Main(string[] args) {
      EvaluationContext buildConfiguration = BuildLoader.Load(Directory.GetCurrentDirectory());
      buildConfiguration.Evaluate(CSharpKeys.Build);
    }
  }
}
