using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Compilation {
  public class RoslynCSharpCompiler : ICSharpCompiler {
    public IObservable<ICompilationResult> Compile(IObservable<IFiles> sourceFiles,
                                                   string outputDir,
                                                   string assemblyName,
                                                   CSharpCompilationOptions options,
                                                   IEnumerable<MetadataReference> references) {
      return sourceFiles.Select(sources => {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var assemblyPath = Path.Combine(outputDir, assemblyName);
        Directory.CreateDirectory(outputDir);
        using (var assemblyOutputFile = File.Create(assemblyPath)) {
          var syntaxTrees = sources.Select(s => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(s)));
          var emitResult = CSharpCompilation.Create(assemblyName, syntaxTrees, references, options).Emit(assemblyOutputFile);
          stopwatch.Stop();
          return new CompilationResult(assemblyPath, emitResult, stopwatch.Elapsed);
        }
      });
    }
  }
}