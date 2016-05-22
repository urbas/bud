using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Scripting {
  public class ScriptBuilder {
    /// <param name="scriptPath">
    ///   the path of the C# script file to build.
    /// </param>
    /// <returns>
    ///   the path to the produced executable.
    ///   The executable can be run as is (using any working directory).
    /// </returns>
    public static string Build(string scriptPath) {
      var script = SyntaxFactory.ParseSyntaxTree(File.ReadAllText(scriptPath), path: scriptPath);
      var scriptCompilation = CSharpCompilation.Create("Bud.Script",
                                                       new[] {script},
                                                       new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location),},
                                                       new CSharpCompilationOptions(OutputKind.ConsoleApplication));
      var outputDir = Path.GetDirectoryName(scriptPath);
      var outputExecutable = Path.Combine(outputDir, "bud.script.exe");
      var emitResult = scriptCompilation.Emit(outputExecutable);
      if (emitResult.Success) {
        return outputExecutable;
      }
      throw new Exception($"Could not compile script '{scriptPath}'. Errors: {string.Join("\n", emitResult.Diagnostics)}");
    }
  }
}