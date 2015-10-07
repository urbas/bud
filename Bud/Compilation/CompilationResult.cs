using Microsoft.CodeAnalysis.Emit;

namespace Bud.Compilation {
  public class CompilationResult : ICompilationResult {
    public string AssemblyPath { get; }
    public EmitResult EmitResult { get; }

    public CompilationResult(string assemblyPath, EmitResult emitResult) {
      AssemblyPath = assemblyPath;
      EmitResult = emitResult;
    }
  }
}