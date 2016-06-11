using System;
using Bud.Scripting;

namespace Bud.ScriptToCsProj {
  public class ScriptToCsProjProgram {
    public static void Main(string[] args) {
      try {
        ScriptCsProj.OutputScriptCsProj(ScriptBuilder.LoadBuiltScriptMetadata());
      } catch (Exception e) {
        Console.Error.WriteLine("Failed to create the csproj file. Error: ");
        Console.Error.WriteLine();
        Console.Error.Write("    ");
        Console.Error.WriteLine(e.Message);
        Environment.Exit(1);
      }
    }
  }
}