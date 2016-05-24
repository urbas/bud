using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.Building;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Scripting {
  public class ScriptBuilder : IFileGenerator {
    /// <param name="scriptPath">
    ///   the path of the C# script file to build.
    /// </param>
    /// <returns>
    ///   the path to the produced executable.
    ///   The executable can be run as is (using any working directory).
    /// </returns>
    public static string Build(string scriptPath) {
      var outputDir = Path.GetDirectoryName(scriptPath);
      var outputExecutable = Path.Combine(outputDir, "bud.script.exe");
      HashBasedBuilder.Build(new ScriptBuilder(), outputExecutable, ImmutableList.Create(scriptPath));
      return outputExecutable;
    }

    public void Generate(string outputFile, IImmutableList<string> inputFiles) {
      var syntaxTrees = inputFiles.Select(script => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(script), path: script));
      var scriptCompilation = CSharpCompilation.Create("Bud.Script",
                                                       syntaxTrees,
                                                       new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)},
                                                       new CSharpCompilationOptions(OutputKind.ConsoleApplication));
      var emitResult = scriptCompilation.Emit(outputFile);
      if (!emitResult.Success) {
        throw new Exception($"Compilation errpr: {string.Join("\n", emitResult.Diagnostics)}");
      }
    }
  }
}