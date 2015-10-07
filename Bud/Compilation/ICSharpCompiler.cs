using System.Collections.Generic;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Compilation {
  public interface ICSharpCompiler {
    ICompilationResult Compile(string targetDir, string assemblyName, IFiles sourceFiles, CSharpCompilationOptions options, IEnumerable<MetadataReference> references);
  }
}