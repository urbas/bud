using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Bud.Building;
using Bud.References;
using Microsoft.CodeAnalysis;
using static Newtonsoft.Json.JsonConvert;

namespace Bud.Scripting {
  public class ScriptBuilder {
    public static readonly Version MaxVersion = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);

    public IReferenceResolver ReferenceResolver { get; }
    public ICSharpScriptCompiler Compiler { get; set; }

    public ScriptBuilder(IReferenceResolver referenceResolver,
                         ICSharpScriptCompiler compiler) {
      ReferenceResolver = referenceResolver;
      Compiler = compiler;
    }

    /// <param name="scriptPath">
    ///   the path of the C# script file to build.
    /// </param>
    /// <param name="referenceResolver">
    ///   these references can be used in the build script in addition to framework references.
    /// </param>
    /// <returns>
    ///   the path to the produced executable.
    ///   The executable can be run as is (using any working directory).
    /// </returns>
    public static string Build(string scriptPath,
                               IReferenceResolver referenceResolver) {
      var compiler = new RoslynCSharpScriptCompiler();
      FilesBuilder scriptBuilder = (inputFiles, outputFile) => Build(inputFiles, referenceResolver, compiler, outputFile);
      var buildDir = Path.Combine(Path.GetDirectoryName(scriptPath), "build");
      Directory.CreateDirectory(buildDir);
      var buildScript = Path.Combine(buildDir, "build-script.exe");
      HashBasedBuilder.Build(scriptBuilder, ImmutableList.Create(scriptPath), buildScript);
      return buildScript;
    }

    public static BuiltScriptMetadata Build(IEnumerable<string> inputFiles,
                                            IReferenceResolver referenceResolver,
                                            ICSharpScriptCompiler compiler,
                                            string outputScriptExe) {
      var outputDir = Path.GetDirectoryName(outputScriptExe);
      var inputFilesList = inputFiles as IList<string> ?? inputFiles.ToList();
      var scriptContents = inputFilesList.Select(File.ReadAllText).ToList();
      var references = ScriptDirectives.Extract(scriptContents);
      var resolvedReferences = ResolveReferences(referenceResolver, references);
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
      var builtScript = new BuiltScriptMetadata(resolvedReferences, outputScriptExe);
      var builtScriptMetadata = SerializeObject(builtScript);
      File.WriteAllText(ScriptMetadataPath(outputScriptExe), builtScriptMetadata);
      return builtScript;
    }

    public static string ScriptMetadataPath(string outputScriptExePath)
      => $"{outputScriptExePath}.metadata.json";

    private static ResolvedScriptReferences ResolveReferences(IReferenceResolver referenceResolver,
                                                              ScriptDirectives directives) {
      var frameworkAssemblies = new Dictionary<string, string>();
      var customAssemblies = new Dictionary<string, string>();
      var resolvedReferences = referenceResolver.Resolve(directives.References);

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
        var destFileName = Path.Combine(outputDir, Path.GetFileName(path));
        HashBasedBuilder.Build(Copy, path, destFileName);
      }
    }

    private static void Copy(string inputFile, string output)
      => File.Copy(inputFile, output, true);
  }
}