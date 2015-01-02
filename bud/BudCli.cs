using System;
using System.IO;
using Bud.Plugins.Build;
using Bud.Commander;

namespace Bud {
  public static class BudCli {
    public static void Main(string[] args) {
      var buildCommander = BuildCommander.Load(Directory.GetCurrentDirectory());
      buildCommander.Evaluate(BuildKeys.Build);
    }
  }
}
