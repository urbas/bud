using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Bud.Building;
using Bud.FrameworkAssemblies;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace Bud.Scripting {
  public class ScriptBuilder {
    public static readonly Version MaxVersion = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
    public IAssemblyPaths AssemblyPaths { get; }
    public ICSharpScriptCompiler Compiler { get; set; }

    public ScriptBuilder(IAssemblyPaths assemblyPaths,
                         ICSharpScriptCompiler compiler) {
      AssemblyPaths = assemblyPaths;
      Compiler = compiler;
    }

    /// <param name="scriptPath">
    ///   the path of the C# script file to build.
    /// </param>
    /// <param name="assemblyPaths">
    ///   these references can be used in the build script in addition to framework references.
    /// </param>
    /// <returns>
    ///   the path to the produced executable.
    ///   The executable can be run as is (using any working directory).
    /// </returns>
    public static string Build(string scriptPath,
                               IAssemblyPaths assemblyPaths) {
      var compiler = new RoslynCSharpScriptCompiler();
      FilesBuilder scriptBuilder = (inputFiles, outputFile) => Build(inputFiles, assemblyPaths.Get(), compiler, outputFile);
      var buildDir = Path.Combine(Path.GetDirectoryName(scriptPath), "build");
      Directory.CreateDirectory(buildDir);
      var buildScript = Path.Combine(buildDir, "build-script.exe");
      HashBasedBuilder.Build(scriptBuilder, ImmutableList.Create(scriptPath), buildScript);
      return buildScript;
    }

    public static BuiltScript LoadBuiltScriptMetadata(string builtScriptPath)
      => JsonConvert.DeserializeObject<BuiltScript>(File.ReadAllText($"{builtScriptPath}.metadata"));

    public static BuiltScript Build(IEnumerable<string> inputFiles,
                                    IReadOnlyDictionary<string, string> customAssemblyPaths,
                                    ICSharpScriptCompiler compiler,
                                    string outputScriptExe) {
      var outputDir = Path.GetDirectoryName(outputScriptExe);
      var inputFilesList = inputFiles as IList<string> ?? inputFiles.ToList();
      var scriptContents = inputFilesList.Select(File.ReadAllText).ToList();
      var references = ScriptMetadata.Extract(scriptContents);
      var resolvedReferences = ResolveReferences(customAssemblyPaths, references);
      var assemblies = resolvedReferences.FrameworkAssemblyReferences.Values
                                         .Concat(resolvedReferences.AssemblyReferences.Values)
                                         .Select(r => MetadataReference.CreateFromFile(r))
                                         .Concat(new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)})
                                         .ToImmutableList();
      var errors = compiler.Compile(inputFilesList, assemblies, outputScriptExe);
      if (errors.Any()) {
        throw new Exception($"Compilation error: {string.Join("\n", errors)}");
      }
      CopyAssemblies(resolvedReferences.AssemblyReferences.Values, outputDir);
      var builtScript = new BuiltScript(resolvedReferences, outputScriptExe);
      var builtScriptMetadata = JsonConvert.SerializeObject(builtScript);
      File.WriteAllText($"{outputScriptExe}.metadata", builtScriptMetadata);
      return builtScript;
    }

    private static ResolvedReferences ResolveReferences(IReadOnlyDictionary<string, string> customAssemblyPaths,
                                                        ScriptMetadata references) {
      var frameworkAssemblies = new Dictionary<string, string>();
      var customAssemblies = new Dictionary<string, string>();
      foreach (var reference in references.AssemblyReferences) {
        var frameworkAssembly = WindowsResolver.ResolveFrameworkAssembly(reference, MaxVersion);
        if (frameworkAssembly.HasValue) {
          frameworkAssemblies.Add(reference, frameworkAssembly.Value);
          continue;
        }
        var customAssembly = customAssemblyPaths.Get(reference);
        if (customAssembly.HasValue) {
          customAssemblies.Add(reference, customAssembly.Value);
          continue;
        }
        throw new Exception($"Could not resolve the reference '{reference}'.");
      }
      return new ResolvedReferences(new ReadOnlyDictionary<string, string>(customAssemblies),
                                    new ReadOnlyDictionary<string, string>(frameworkAssemblies));
    }

    public static string DefaultScriptPath
      => Path.Combine(Directory.GetCurrentDirectory(), "Build.cs");

    private static void CopyAssemblies(IEnumerable<string> paths,
                                       string outputDir) {
      foreach (var path in paths) {
        var destFileName = Path.Combine(outputDir, Path.GetFileName(path));
        HashBasedBuilder.Build(Copy, path, destFileName);
      }
    }

    private static void Copy(string inputFile, string output)
      => File.Copy(inputFile, output, true);
  }
}