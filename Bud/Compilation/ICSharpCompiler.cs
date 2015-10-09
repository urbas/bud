using System;
using System.Collections.Generic;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Compilation {
  public interface ICSharpCompiler {
    IObservable<ICompilationResult> Compile(IObservable<FilesUpdate> sourceFiles,
                                            string outputDir,
                                            string assemblyName,
                                            CSharpCompilationOptions options,
                                            IEnumerable<MetadataReference> references);
  }
}