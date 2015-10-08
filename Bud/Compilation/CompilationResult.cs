using System;
using Microsoft.CodeAnalysis.Emit;

namespace Bud.Compilation {
  public class CompilationResult : ICompilationResult {
    public string AssemblyPath { get; }
    public EmitResult EmitResult { get; }
    public TimeSpan CompilationTime { get; }

    public CompilationResult(string assemblyPath, EmitResult emitResult, TimeSpan compilationTime) {
      AssemblyPath = assemblyPath;
      EmitResult = emitResult;
      CompilationTime = compilationTime;
    }
  }
}