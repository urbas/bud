using System;
using System.Collections.Generic;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Compilation {
  public interface ICSharpCompiler {
    IObservable<CSharpCompilation> Compile(IObservable<IEnumerable<string>> sourceFiles, IObservable<IEnumerable<MetadataReference>> observedReferences, string assemblyName, CSharpCompilationOptions options);
  }
}