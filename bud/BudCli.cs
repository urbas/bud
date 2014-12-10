using System;
using System.IO;
using Bud.Plugins.CSharp;
using Bud.Plugins.BuildLoading;

namespace Bud {
  public static class BudCli {
    public static void Main(string[] args) {
      var buildCommandInvoker = BuildLoading.Load(Directory.GetCurrentDirectory());
      buildCommandInvoker.Evaluate(CSharpKeys.Build.ToString());
    }
  }
}
