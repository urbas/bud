using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.Building;
using Bud.Cache;
using Bud.FrameworkAssemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Scripting {
  public class ScriptBuilder : IDirContentGenerator {
    /// <param name="scriptPath">
    ///   the path of the C# script file to build.
    /// </param>
    /// <returns>
    ///   the path to the produced executable.
    ///   The executable can be run as is (using any working directory).
    /// </returns>
    public static string Build(string scriptPath) {
      var thisScriptCache = Path.Combine(Path.GetTempPath(), "Bud", "cache", "scripts", Path.GetFileName(Path.GetDirectoryName(scriptPath)));
      Directory.CreateDirectory(thisScriptCache);
      var outputDir = CachingHashBasedBuilder.Build(new ScriptBuilder(),
                                                    new HashBasedCache(thisScriptCache),
                                                    ImmutableList.Create(scriptPath),
                                                    new byte[] {});
      return Path.Combine(outputDir, "script.exe");
    }

    public void Generate(string outputDir, IImmutableList<string> inputFiles) {
      var outputScript = Path.Combine(outputDir, "script.exe");
      var scriptContents = inputFiles.Select(File.ReadAllText).ToList();
      var references = scriptContents.Select(ScriptReferences.Parse)
                                     .SelectMany(refs => refs)
                                     .Select(assemblyName => WindowsResolver.ResolveFrameworkAssembly(assemblyName, Version.Parse("4.6.0.0")))
                                     .Gather()
                                     .Select(path => MetadataReference.CreateFromFile(path))
                                     .ToImmutableList();
      var syntaxTrees = inputFiles.Select(script => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(script), path: script));
      var scriptCompilation = CSharpCompilation.Create("Bud.Script",
                                                       syntaxTrees,
                                                       references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location)),
                                                       new CSharpCompilationOptions(OutputKind.ConsoleApplication));
      var emitResult = scriptCompilation.Emit(outputScript);
      if (!emitResult.Success) {
        throw new Exception($"Compilation errpr: {string.Join("\n", emitResult.Diagnostics)}");
      }
    }
  }
}