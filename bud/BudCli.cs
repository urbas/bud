using System;
using System.IO;
using Bud.Plugins.CSharp;

namespace Bud {
  class BudCli {
    public static void Main(string[] args) {
      BuildConfiguration buildConfiguration = BuildConfigurationLoader.Load(Directory.GetCurrentDirectory());
      buildConfiguration.Evaluate(CSharpPlugin.CSharpBuild);
    }
  }
}
