using System;
using System.IO;
using Bud.Scripting;

namespace Bud.RunScript {
  public class Program {
    public static void Main(string[] args) {
      try {
        var exitCode = ScriptRunner.Run(Path.Combine(Directory.GetCurrentDirectory(), "Build.cs"),
                                        args,
                                        Directory.GetCurrentDirectory());
        Environment.Exit(exitCode);
      } catch (Exception e) {
        Console.Error.WriteLine("An error occurred when trying to run the script. Error message:\n\n    " + e.Message);
        Environment.Exit(1);
      }
    }
  }
}