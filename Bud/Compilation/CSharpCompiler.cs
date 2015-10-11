using System;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Compilation {
  public static class CSharpCompiler {
    public static IObservable<CSharpCompilation> CompileWith(this IObservable<CSharpCompilationInput> input, ICSharpCompiler compiler, IConfigs config)
      => compiler.Compile(input, config);
  }
}