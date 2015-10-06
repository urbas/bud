using Microsoft.CodeAnalysis.Emit;

namespace Bud {
  public class CompilationResult {
    public string AssemblyPath { get; }
    public EmitResult EmitResult { get; }

    public CompilationResult(string assemblyPath, EmitResult emitResult) {
      AssemblyPath = assemblyPath;
      EmitResult = emitResult;
    }
  }
}