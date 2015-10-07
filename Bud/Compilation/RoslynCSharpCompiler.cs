using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Compilation {
  public class RoslynCSharpCompiler : ICSharpCompiler {
    public ICompilationResult Compile(string outputDir, string assemblyName, IFiles sourceFiles, CSharpCompilationOptions options, IEnumerable<MetadataReference> references) {
      var assemblyPath = Path.Combine(outputDir, assemblyName);
      using (var assemblyOutputFile = File.Create(assemblyPath)) {
        return new CompilationResult(
          assemblyPath,
          CSharpCompilation.Create(assemblyName,
                                   sourceFiles.Select(s => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(s))),
                                   references,
                                   options)
                           .Emit(assemblyOutputFile));
      }
    }
  }
}