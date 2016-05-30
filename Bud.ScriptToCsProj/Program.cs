using System;
using System.IO;
using System.Linq;
using Bud.Scripting;

namespace Bud.ScriptToCsProj {
  public class Program {
    public static void Main(string[] args) {
      var scriptContents = new[] {ScriptBuilder.DefaultScriptPath}
        .Select(File.ReadAllText);
      var buildScriptReferences = ScriptReferences
        .Extract(scriptContents)
        .Select(r => new Reference(r, BudAssemblyPaths.Get(r)));
      Console.Write(ScriptCsProj.BudScriptCsProj(buildScriptReferences, Directory.GetCurrentDirectory()));
    }
  }
}