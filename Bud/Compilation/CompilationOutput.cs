using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Compilation {
  public class CompilationOutput {
    public CSharpCompilation Compilation { get; }

    public CompilationOutput(CSharpCompilation compilation) {
      Compilation = compilation;
    }
  }
}