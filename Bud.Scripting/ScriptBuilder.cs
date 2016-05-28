using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.Building;
using Bud.Cache;
using Bud.FrameworkAssemblies;
using Microsoft.CodeAnalysis;

namespace Bud.Scripting {
  public class ScriptBuilder : IDirContentGenerator {
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
      var thisScriptCache = Path.Combine(Path.GetTempPath(), "Bud", "cache", "scripts", Path.GetFileName(Path.GetDirectoryName(scriptPath)));
      Directory.CreateDirectory(thisScriptCache);
      var scriptBuilder = new ScriptBuilder(availableReferences,
                                            new RoslynCSharpScriptCompiler());
      var outputDir = CachingHashBasedBuilder.Build(scriptBuilder,
                                                    new HashBasedCache(thisScriptCache),
                                                    ImmutableList.Create(scriptPath),
                                                    new byte[] {});
      return Path.Combine(outputDir, "script.exe");
    }

    public void Generate(string outputDir, IImmutableList<string> inputFiles) {
      var outputScript = Path.Combine(outputDir, "script.exe");
      var scriptContents = inputFiles.Select(File.ReadAllText).ToList();
      var customAssemblyReferences = ExtractReferences(scriptContents, AvailableReferences.Get()).ToList();
      var assemblies = ResolveFrameworkReferences(customAssemblyReferences)
        .Select(r => MetadataReference.CreateFromFile(r.Path.Value))
                                 .Concat(new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) })
                                 .ToImmutableList();
//      var references = scriptContents.Select(ScriptReferences.Parse)
//                                     .SelectMany(refs => refs)
//                                     .ToImmutableHashSet();
//      var availableCustomReferences = AvailableReferences.Get();
//      var customAssemblyPaths = references.Select(reference => availableCustomReferences.Get(reference)).Gather().ToList();
//      var customAssemblies = customAssemblyPaths.Select(path => MetadataReference.CreateFromFile(path));
//      var assemblies = references.Except(availableCustomReferences.Keys)
//                                 .Select(ResolveAssembly)
//                                 .Select(path => MetadataReference.CreateFromFile(path))
//                                 .Concat(new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)})
//                                 .Concat(customAssemblies)
//                                 .ToImmutableList();
      var errors = Compiler.Compile(inputFiles, assemblies, outputScript);
      if (errors.Any()) {
        throw new Exception($"Compilation error: {string.Join("\n", errors)}");
      }
      foreach (var path in customAssemblyReferences.Where(r => r.Path.HasValue)) {
        File.Copy(path.Path.Value, Path.Combine(outputDir, Path.GetFileName(path.Path.Value)));
      }
    }

    public static IEnumerable<Reference> ExtractReferences(IEnumerable<string> scriptContents,
                                                           IReadOnlyDictionary<string, string> availableReferences)
      => scriptContents.Select(ScriptReferences.Parse)
                       .SelectMany(refs => refs)
                       .ToImmutableHashSet()
                       .Select(r => new Reference(r, availableReferences.Get(r)));

    public static IEnumerable<Reference> ResolveFrameworkReferences(IEnumerable<Reference> unresolvedReferences)
      => unresolvedReferences.Select(r => r.Path.HasValue ? r : new Reference(r.AssemblyName, ResolveAssembly(r.AssemblyName)));

    public static string GetDefaultScriptPath()
      => Path.Combine(Directory.GetCurrentDirectory(), "Build.cs");

    private static string ResolveAssembly(string assemblyName) {
      var assembly = WindowsResolver.ResolveFrameworkAssembly(assemblyName, FrameworkAssembliesVersion);
      if (assembly.HasValue) {
        return assembly.Value;
      }
      throw new Exception($"Could not resolve the reference '{assemblyName}'.");
    }
  }
}