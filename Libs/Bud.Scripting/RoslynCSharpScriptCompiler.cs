using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.References;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Scripting {
  public class RoslynCSharpScriptCompiler : ICSharpScriptCompiler {
    public ImmutableArray<Diagnostic> Compile(ImmutableArray<string> inputFiles, ResolvedReferences references, string outputExe)
      => CSharpCompilation.Create("Bud.Script",
                                  inputFiles.Select(ParseSyntaxTree),
                                  ToMetadataReferences(references),
                                  new CSharpCompilationOptions(OutputKind.ConsoleApplication))
                          .Emit(outputExe)
                          .Diagnostics;

    private static IEnumerable<PortableExecutableReference> ToMetadataReferences(ResolvedReferences references) {
      var assemblyPaths = references.Assemblies.Select(assemblyPath => assemblyPath.Path);

      return FrameworkAssemblyPaths(references.FrameworkAssemblies)
        .Concat(assemblyPaths)
        .Select(r => MetadataReference.CreateFromFile(r))
        .Concat(new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)});
    }

    private static SyntaxTree ParseSyntaxTree(string script)
      => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(script), path: script);

    private static IEnumerable<string> FrameworkAssemblyPaths(IEnumerable<FrameworkAssembly> frameworkAssemblyReferences)
      => frameworkAssemblyReferences
        .Select(frameworkAssemblyReference => {
          var frameworkAssembly = WindowsFrameworkReferenceResolver.ResolveFrameworkAssembly(frameworkAssemblyReference.Name, frameworkAssemblyReference.FrameworkVersion);
          if (frameworkAssembly.HasValue) {
            return frameworkAssembly.Value;
          }
          throw new Exception($"Could not resolve the reference '{frameworkAssemblyReference.Name}'.");
        });
  }
}