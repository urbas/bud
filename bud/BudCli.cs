using System;
using System.IO;
using Bud.Plugins.CSharp;

namespace Bud {
  class BudCli {
    public static void Main(string[] args) {
      EvaluationContext buildConfiguration = BuildConfigurationLoader.Load(Directory.GetCurrentDirectory());
      buildConfiguration.Evaluate(CSharpPlugin.CSharpBuild);
    }
  }
}
