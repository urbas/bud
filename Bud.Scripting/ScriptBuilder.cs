using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Bud.Building;
using Bud.NuGet;
using Bud.References;
using Microsoft.CodeAnalysis;
using static Newtonsoft.Json.JsonConvert;

namespace Bud.Scripting {
  public class ScriptBuilder {
    public static readonly Version MaxVersion = new Version(Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue);

    public IReferenceResolver ReferenceResolver { get; }
    public ICSharpScriptCompiler Compiler { get; set; }

    public ScriptBuilder(IReferenceResolver referenceResolver,
                         ICSharpScriptCompiler compiler) {
      ReferenceResolver = referenceResolver;
      Compiler = compiler;
    }

    /// <summary>
    ///   Loads the metadata of the script in the current working directory.
    ///   This method will build the script first (as the metadata is available only for
    ///   built scripts).
    /// </summary>
    /// <param name="scriptPath">
    ///   the path of the script that will be built and whose metadata we seek.
    /// </param>
    /// <returns>the metadata.</returns>
    public static BuiltScriptMetadata LoadBuiltScriptMetadata(Option<string> scriptPath = default(Option<string>)) {
      var scriptMetadataPath = ScriptMetadataPath(Build(scriptPath));
      return DeserializeObject<BuiltScriptMetadata>(File.ReadAllText(scriptMetadataPath));
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
      => Build(inputFiles, new BudReferenceResolver(), new RoslynCSharpScriptCompiler(), outputFile);

    internal static BuiltScriptMetadata Build(IEnumerable<string> inputFiles,
                                              IReferenceResolver referenceResolver,
                                              ICSharpScriptCompiler compiler,
                                              string outputScriptExe) {
      var outputDir = Path.GetDirectoryName(outputScriptExe);
      var inputFilesList = inputFiles as IList<string> ?? inputFiles.ToList();
      var scriptContents = inputFilesList.Select(File.ReadAllText).ToList();
      var directives = ScriptDirectives.Extract(scriptContents);

      var packagesConfigFile = Path.Combine(outputDir, "packages.config");
      using (var packagesConfigFileStream = new FileStream(packagesConfigFile, FileMode.Create, FileAccess.Write)) {
        using (var packageConfigFileWriter = new StreamWriter(packagesConfigFileStream)) {
          PackageReference.WritePackagesConfigXml(directives.NuGetReferences,
                                                  packageConfigFileWriter);
        }
      }

      var resolvedReferences = ResolveReferences(referenceResolver, directives.References);
      var assemblies = resolvedReferences.FrameworkAssemblyReferences.Values
                                         .Concat(resolvedReferences.AssemblyReferences.Values)
                                         .Select(r => MetadataReference.CreateFromFile(r))
                                         .Concat(new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)})
                                         .ToImmutableList();
      var errors = compiler.Compile(inputFilesList, assemblies, outputScriptExe);
      if (errors.Any()) {
        throw new Exception($"Compilation error: {String.Join("\n", errors)}");
      }
      CopyAssemblies(resolvedReferences.AssemblyReferences.Values, outputDir);
      var builtScript = new BuiltScriptMetadata(resolvedReferences, outputScriptExe);
      var builtScriptMetadata = SerializeObject(builtScript);
      File.WriteAllText(ScriptMetadataPath(outputScriptExe), builtScriptMetadata);
      return builtScript;
    }

    public static string ScriptMetadataPath(string outputScriptExePath)
      => $"{outputScriptExePath}.metadata.json";

    private static ResolvedScriptReferences ResolveReferences(IReferenceResolver referenceResolver,
                                                              IEnumerable<string> references) {
      var frameworkAssemblies = new Dictionary<string, string>();
      var customAssemblies = new Dictionary<string, string>();
      var resolvedReferences = referenceResolver.Resolve(references);

      foreach (var reference in resolvedReferences) {
        if (reference.Value.HasValue) {
          customAssemblies.Add(reference.Key, reference.Value.Value);
        } else {
          var frameworkAssembly = WindowsFrameworkReferenceResolver.ResolveFrameworkAssembly(reference.Key, MaxVersion);
          if (frameworkAssembly.HasValue) {
            frameworkAssemblies.Add(reference.Key, frameworkAssembly.Value);
          } else {
            throw new Exception($"Could not resolve the reference '{reference.Key}'.");
          }
        }
      }
      return new ResolvedScriptReferences(new ReadOnlyDictionary<string, string>(customAssemblies),
                                          new ReadOnlyDictionary<string, string>(frameworkAssemblies));
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