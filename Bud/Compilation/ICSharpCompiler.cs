using System;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Compilation {
  public interface ICSharpCompiler {
    IObservable<CSharpCompilation> Compile(IObservable<CSharpCompilationInput> input, IConfigs config);
  }
}