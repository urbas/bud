using System;
using System.IO;
using System.Linq;
using Bud.Scripting;

namespace Bud.ScriptToCsProj {
  public class Program {
    public static void Main(string[] args) {
      var executable = ScriptBuilder.Build(ScriptBuilder.DefaultScriptPath, new BudAssemblyPaths());
      var builtScriptMetadata = ScriptBuilder.LoadBuiltScriptMetadata(executable);
      var nonFrameworkReferences = builtScriptMetadata.ResolvedReferences
                                                      .AssemblyReferences
                                                      .Select(pair => new CsProjReference(pair.Key, pair.Value));
      var frameworkReferences = builtScriptMetadata.ResolvedReferences
                                                   .FrameworkAssemblyReferences
                                                   .Select(pair => new CsProjReference(pair.Key));
      Console.Write(ScriptCsProj.BudScriptCsProj(nonFrameworkReferences.Concat(frameworkReferences),
                                                 Directory.GetCurrentDirectory()));
    }
  }
}