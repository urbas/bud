using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.Building;
using Bud.NuGet;
using Bud.References;
using Newtonsoft.Json;
using static Newtonsoft.Json.JsonConvert;

namespace Bud.Scripting {
  public class ScriptBuilder {
    /// <summary>
    ///   Loads the metadata of the script in the current working directory.
    ///   This method will build the script first (as the metadata is available only for
    ///   built scripts).
    /// </summary>
    /// <param name="scriptPath">
    ///   the path to the script executable. The "built script metadata" JSON file
    ///   is placed next to the script executable.
    /// </param>
    /// <returns>the metadata of the script.</returns>
    public static BuiltScriptMetadata LoadBuiltScriptMetadata(Option<string> scriptPath = default(Option<string>)) {
      var scriptMetadataPath = ScriptMetadataPath(Build(scriptPath));
      return DeserializeObject<BuiltScriptMetadata>(File.ReadAllText(scriptMetadataPath));
    }

    /// <summary>
    ///   Stores a json file next to the given script executable.
    /// </summary>
    /// <param name="scriptPath">
    ///   the path of the script that will be built and whose metadata we seek.
    /// </param>
    /// <param name="references">
    ///   a list of non-framework assemblies and framework assemblies referenced by the script.
    /// </param>
    /// <returns></returns>
    public static BuiltScriptMetadata SaveBuiltScriptMetadata(string scriptPath, ResolvedReferences references) {
      var builtScriptMetadata = new BuiltScriptMetadata(references);
      var builtScriptMetadataJson = SerializeObject(builtScriptMetadata, Formatting.Indented);
      File.WriteAllText(ScriptMetadataPath(scriptPath), builtScriptMetadataJson);
      return builtScriptMetadata;
    }

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
                                    ImmutableList.Create(actualScriptPath),
                                    Path.Combine(buildDir, "build-script.exe"));
    }

    private static void Build(IReadOnlyList<string> inputFiles, string outputFile)
      => Build(inputFiles,
               new BudReferenceResolver(),
               new RoslynCSharpScriptCompiler(),
               new NuGetReferenceResolver(),
               outputFile);

    internal static BuiltScriptMetadata Build(IEnumerable<string> inputFiles,
                                              IReferenceResolver referenceResolver,
                                              ICSharpScriptCompiler compiler,
                                              INuGetReferenceResolver nuGetReferenceResolver,
                                              string outputScriptExe) {
      var outputDir = Path.GetDirectoryName(outputScriptExe);
      var inputFilesList = inputFiles as IList<string> ?? inputFiles.ToList();

      var references = ResolveReferences(referenceResolver, nuGetReferenceResolver, inputFilesList, outputDir);
      Compile(compiler, references, inputFilesList, outputScriptExe);
      CopyAssemblies(references.Assemblies
                               .Select(assemblyPath => assemblyPath.Path), outputDir);
      return SaveBuiltScriptMetadata(outputScriptExe, references);
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
        .Add(ResolveReferences(referenceResolver, directives.References));
    }

    private static void Compile(ICSharpScriptCompiler compiler,
                                ResolvedReferences references,
                                IEnumerable<string> inputFilesList,
                                string outputFile) {
      var errors = compiler.Compile(inputFilesList, references, outputFile);
      if (errors.Any()) {
        throw new Exception($"Compilation error: {string.Join("\n", errors)}");
      }
    }

    private static ResolvedReferences ResolveReferences(IReferenceResolver referenceResolver,
                                                        IEnumerable<string> references) {
      var frameworkAssemblies = new List<FrameworkAssembly>();
      var customAssemblies = new List<Assembly>();
      var resolvedReferences = referenceResolver.Resolve(references);

      foreach (var reference in resolvedReferences) {
        if (reference.Value.HasValue) {
          customAssemblies.Add(new Assembly(reference.Key, reference.Value.Value));
        } else {
          frameworkAssemblies.Add(new FrameworkAssembly(reference.Key, FrameworkAssembly.MaxVersion));
        }
      }
      return new ResolvedReferences(customAssemblies, frameworkAssemblies);
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