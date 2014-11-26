using System;
using System.IO;

namespace Bud {
  class BudCli {
    public static void Main(string[] args) {
      BuildConfiguration buildConfiguration = Bud.Load(Directory.GetCurrentDirectory());
      Console.WriteLine(args[0]);
      Bud.Evaluate(buildConfiguration, args[0]);
    }
  }
}
