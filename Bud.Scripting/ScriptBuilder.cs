using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.Building;
using Bud.NuGet;
using Bud.References;

namespace Bud.Scripting {
  public class ScriptBuilder {
    /// <summary>
    ///   The default way of building <c>Build.cs</c> scripts.
    /// </summary>
    /// <param name="scriptPath">
    ///   the path of the script to build. If omitted, <c>Build.cs</c>
    ///   in the current working directory is used.
    /// </param>
    /// <returns>the path to the built executable. This executable can be run as is.</returns>
    public static string Build(Option<string> scriptPath = default(Option<string>)) {
      var actualScriptPath = scriptPath.HasValue ? scriptPath.Value : ScriptRunner.DefaultScriptPath;
      var buildDir = CreateBuildDir(actualScriptPath);
      return HashBasedBuilder.Build(Build,
                                    ImmutableArray.Create(actualScriptPath),
                                    Path.Combine(buildDir, "build-script.exe"));
    }

    private static void Build(ImmutableArray<string> inputFiles, string outputFile)
      => Build(inputFiles,
               new BudReferenceResolver(),
               new RoslynCSharpScriptCompiler(),
               new NuGetReferenceResolver(),
               outputFile);

    internal static BuiltScriptMetadata Build(ImmutableArray<string> inputFiles,
                                              IReferenceResolver referenceResolver,
                                              ICSharpScriptCompiler compiler,
                                              INuGetReferenceResolver nuGetReferenceResolver,
                                              string outputScriptExe) {
      var outputDir = Path.GetDirectoryName(outputScriptExe);

      var references = ResolveReferences(referenceResolver, nuGetReferenceResolver, inputFiles, outputDir);
      Compile(compiler, references, inputFiles, outputScriptExe);
      CopyAssemblies(references.Assemblies
                               .Select(assemblyPath => assemblyPath.Path), outputDir);
      return BuiltScriptMetadata.Save(outputScriptExe, references);
    }

    public static string ScriptMetadataPath(string outputScriptExePath)
      => $"{outputScriptExePath}.metadata.json";

    private static ResolvedReferences ResolveReferences(IReferenceResolver referenceResolver,
                                                        INuGetReferenceResolver nuGetReferenceResolver,
                                                        IEnumerable<string> inputFilesList,
                                                        string downloadedPackagesDir) {
      var scriptContents = inputFilesList.Select(File.ReadAllText).ToList();
      var directives = ScriptDirectives.Extract(scriptContents);

      return nuGetReferenceResolver
        .Resolve(directives.NuGetReferences, downloadedPackagesDir)
        .Add(referenceResolver.Resolve(directives.References));
    }

    private static void Compile(ICSharpScriptCompiler compiler,
                                ResolvedReferences references,
                                ImmutableArray<string> inputFilesList,
                                string outputFile) {
      var errors = compiler.Compile(inputFilesList, references, outputFile);
      if (errors.Any()) {
        throw new Exception($"Compilation error: {string.Join("\n", errors)}");
      }
    }

    private static void CopyAssemblies(IEnumerable<string> paths,
                                       string outputDir) {
      foreach (var path in paths) {
        Copy(path, Path.Combine(outputDir, Path.GetFileName(path)));
      }
    }

    private static void Copy(string inputFile, string output)
      => File.Copy(inputFile, output, true);

    private static string CreateBuildDir(string scriptPath) {
      var buildDir = Path.Combine(Path.GetDirectoryName(scriptPath), "build");
      Directory.CreateDirectory(buildDir);
      return buildDir;
    }
  }
}