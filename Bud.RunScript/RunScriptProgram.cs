using System;
using Bud.Scripting;

namespace Bud.RunScript {
  public class RunScriptProgram {
    public static void Main(string[] args) {
      try {
        var exitCode = ScriptRunner.Run(args);
        Environment.Exit(exitCode);
      } catch (Exception e) {
        Console.Error.WriteLine($"An error occurred while trying to run the script. Error message:\n\n    {e}");
        Environment.Exit(1);
      }
    }
  }
}