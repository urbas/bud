using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.Make;
using Bud.NuGet;
using Bud.References;

namespace Bud.Scripting {
  public class ScriptBuilder {
    /// <summary>
    ///   The name of the compiled build script executable.
    /// </summary>
    public const string BuildScriptExeFileName = "build-script.exe";

    /// <summary>
    ///   The name of the directorywhere the compiled build script will be placed.
    /// </summary>
    public const string ScriptBuildDirName = ".bud";

    /// <summary>
    ///   The default way of building <c>Build.cs</c> scripts.
    ///   This method places all the output in the <see cref="ScriptBuildDirName" /> directory.
    ///   This directory will be placed next to the input file.
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
                                    Path.Combine(buildDir, BuildScriptExeFileName));
    }

    public static string ScriptMetadataPath(string outputScriptExePath)
      => $"{outputScriptExePath}.metadata.json";

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
      var scriptDirectives = ScriptDirectives.Extract(inputFiles.Select(File.ReadAllText));
      var references = nuGetReferenceResolver.Resolve(scriptDirectives.NuGetReferences, outputDir)
                                             .Add(referenceResolver.Resolve(scriptDirectives.References));

      var taskGraph = TaskGraph.ToTaskGraph(
        () => Compile(compiler, references, inputFiles, outputScriptExe),
        () => CopyAssemblies(references.Assemblies.Select(assemblyPath => assemblyPath.Path), outputDir));
      taskGraph.Run();
      return BuiltScriptMetadata.Save(outputScriptExe, references);
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

    private static void CopyAssemblies(IEnumerable<string> paths, string outputDir) {
      foreach (var path in paths) {
        File.Copy(path, Path.Combine(outputDir, Path.GetFileName(path)), true);
      }
    }

    private static string CreateBuildDir(string scriptPath) {
      var buildDir = Path.Combine(Path.GetDirectoryName(scriptPath), ScriptBuildDirName);
      Directory.CreateDirectory(buildDir);
      return buildDir;
    }
  }
}