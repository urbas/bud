using Microsoft.CodeAnalysis.Emit;

namespace Bud.Compilation {
  public interface ICompilationResult {
    string AssemblyPath { get; }
    EmitResult EmitResult { get; }
  }
}