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
    private static readonly Version MaxVersion = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
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
      var scriptBuilder = new ScriptBuilder(assemblyPaths,
                                            new RoslynCSharpScriptCompiler());
      var buildDir = Path.Combine(Path.GetDirectoryName(scriptPath), "build");
      Directory.CreateDirectory(buildDir);
      var buildScript = Path.Combine(buildDir, "build-script.exe");
      HashBasedBuilder.Build(scriptBuilder, buildScript, ImmutableList.Create(scriptPath));
      return buildScript;
    }

    public void Generate(string outputScriptExe, IImmutableList<string> inputFiles) {
      var customAssemblyPaths = AssemblyPaths.Get();
      var outputDir = Path.GetDirectoryName(outputScriptExe);
      var scriptContents = inputFiles.Select(File.ReadAllText).ToList();
      var references = ScriptReferences.Extract(scriptContents);
      var frameworkAssemblies = new List<string>();
      var customAssemblies = new List<string>();
      foreach (var reference in references) {
        var frameworkAssembly = WindowsResolver.ResolveFrameworkAssembly(reference, MaxVersion);
        if (frameworkAssembly.HasValue) {
          frameworkAssemblies.Add(frameworkAssembly.Value);
          continue;
        }
        var customAssembly = customAssemblyPaths.Get(reference);
        if (customAssembly.HasValue) {
          customAssemblies.Add(customAssembly.Value);
          continue;
        }
        throw new Exception($"Could not resolve the reference '{reference}'.");
      }
      var assemblies = frameworkAssemblies
        .Concat(customAssemblies)
        .Select(r => MetadataReference.CreateFromFile(r))
        .Concat(new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)})
        .ToImmutableList();
      var errors = Compiler.Compile(inputFiles, assemblies, outputScriptExe);
      if (errors.Any()) {
        throw new Exception($"Compilation error: {string.Join("\n", errors)}");
      }
      CopyAssemblies(customAssemblies, outputDir);
    }

    public static string DefaultScriptPath
      => Path.Combine(Directory.GetCurrentDirectory(), "Build.cs");

    private static void CopyAssemblies(IEnumerable<string> paths,
                                       string outputDir) {
      foreach (var path in paths) {
        var destFileName = Path.Combine(outputDir, Path.GetFileName(path));
        HashBasedBuilder.Build(CopyFirstFileTo, destFileName, ImmutableList.Create(path));
      }
    }

    private static void CopyFirstFileTo(string output, IImmutableList<string> input) {
      File.Delete(output);
      File.Copy(input[0], output);
    }
  }
}