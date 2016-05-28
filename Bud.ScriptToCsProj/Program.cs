using System;
using System.IO;
using System.Linq;
using Bud.Scripting;

namespace Bud.ScriptToCsProj {
  public class Program {
    public static void Main(string[] args) {
      var buildScriptReferences = ScriptBuilder.ExtractReferences(
        new[] {ScriptBuilder.GetDefaultScriptPath()}.Select(File.ReadAllText),
        new BudReferences().Get());
      Console.Write(ScriptCsProj.BudScriptCsProj(buildScriptReferences));
    }
  }
}