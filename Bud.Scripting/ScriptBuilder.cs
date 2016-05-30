using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.Building;
using Bud.FrameworkAssemblies;
using Microsoft.CodeAnalysis;

namespace Bud.Scripting {
  public class ScriptBuilder : IDirContentGenerator, IFileGenerator {
    private static readonly Version FrameworkAssembliesVersion = Version.Parse("4.6.0.0");
    public IAssemblyReferences AvailableReferences { get; }
    public ICSharpScriptCompiler Compiler { get; set; }

    public ScriptBuilder(IAssemblyReferences availableReferences,
                         ICSharpScriptCompiler compiler) {
      AvailableReferences = availableReferences;
      Compiler = compiler;
    }

    /// <param name="scriptPath">
    ///   the path of the C# script file to build.
    /// </param>
    /// <param name="availableReferences">
    ///   these references can be used in the build script in addition to framework references.
    /// </param>
    /// <returns>
    ///   the path to the produced executable.
    ///   The executable can be run as is (using any working directory).
    /// </returns>
    public static string Build(string scriptPath,
                               IAssemblyReferences availableReferences) {
      var scriptBuilder = new ScriptBuilder(availableReferences,
                                            new RoslynCSharpScriptCompiler());
      var buildDir = Path.Combine(Path.GetDirectoryName(scriptPath), "build");
      Directory.CreateDirectory(buildDir);
      string buildScript = Path.Combine(buildDir, "build-script.exe");
      HashBasedBuilder.Build(scriptBuilder, buildScript, ImmutableList.Create(scriptPath));
      return buildScript;
    }

    public void Generate(string outputScriptExe, IImmutableList<string> inputFiles) {
      var outputDir = Path.GetDirectoryName(outputScriptExe);
      var scriptContents = inputFiles.Select(File.ReadAllText).ToList();
      var customAssemblyReferences = ExtractReferences(scriptContents, AvailableReferences.Get()).ToList();
      var assemblies = ResolveFrameworkReferences(customAssemblyReferences)
        .Select(r => MetadataReference.CreateFromFile(r.Path.Value))
        .Concat(new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)})
        .ToImmutableList();
      var errors = Compiler.Compile(inputFiles, assemblies, outputScriptExe);
      if (errors.Any()) {
        throw new Exception($"Compilation error: {string.Join("\n", errors)}");
      }
      CopyAssemblies(customAssemblyReferences, outputDir);
    }

    public static IEnumerable<Reference> ExtractReferences(IEnumerable<string> scriptContents,
                                                           IReadOnlyDictionary<string, string> availableReferences)
      => scriptContents.Select(ScriptReferences.Parse)
                       .SelectMany(refs => refs)
                       .ToImmutableHashSet()
                       .Select(r => new Reference(r, availableReferences.Get(r)));

    public static string GetDefaultScriptPath()
      => Path.Combine(Directory.GetCurrentDirectory(), "Build.cs");

    private static void CopyAssemblies(IEnumerable<Reference> customAssemblyReferences, 
                                       string outputDir) {
      foreach (var path in customAssemblyReferences.Gather(r => r.Path)) {
        var destFileName = Path.Combine(outputDir, Path.GetFileName(path));
        HashBasedBuilder.Build((output, input) => {
          File.Delete(destFileName);
          File.Copy(path, destFileName);
        }, destFileName, ImmutableList.Create(path));
      }
    }

    private static string ResolveAssembly(string assemblyName) {
      var assembly = WindowsResolver.ResolveFrameworkAssembly(assemblyName, FrameworkAssembliesVersion);
      if (assembly.HasValue) {
        return assembly.Value;
      }
      throw new Exception($"Could not resolve the reference '{assemblyName}'.");
    }

    private static IEnumerable<Reference> ResolveFrameworkReferences(IEnumerable<Reference> unresolvedReferences)
      => unresolvedReferences.Select(r => r.Path.HasValue ? r : new Reference(r.AssemblyName, ResolveAssembly(r.AssemblyName)));
  }
}