using System;
using System.Collections.Generic;
using Bud.IO;
using Microsoft.CodeAnalysis;

namespace Bud.Cs {
  public class CompileOutput {
    public MetadataReference Reference { get; }
    public long Timestamp { get; }
    public IEnumerable<Diagnostic> Diagnostics { get; }
    public TimeSpan CompilationTime { get; }
    public string AssemblyPath { get; }
    public bool Success { get; }

    public CompileOutput(IEnumerable<Diagnostic> diagnostics,
                         TimeSpan compilationTime,
                         string assemblyPath,
                         bool success,
                         long timestamp,
                         MetadataReference reference) {
      Diagnostics = diagnostics;
      CompilationTime = compilationTime;
      AssemblyPath = assemblyPath;
      Success = success;
      Timestamp = timestamp;
      Reference = reference;
    }

    public IAssemblyReference ToAssemblyReference()
      => new AssemblyReference(AssemblyPath, Reference);

    protected bool Equals(CompileOutput other)
      => Timestamp == other.Timestamp && string.Equals(AssemblyPath, other.AssemblyPath);

    public override bool Equals(object obj)
      => !ReferenceEquals(null, obj) &&
         (ReferenceEquals(this, obj) ||
          obj.GetType() == GetType() &&
          Equals((CompileOutput) obj));

    public override int GetHashCode() {
      unchecked {
        return (Timestamp.GetHashCode() * 397) ^ AssemblyPath.GetHashCode();
      }
    }

    public static bool operator ==(CompileOutput left, CompileOutput right) => Equals(left, right);
    public static bool operator !=(CompileOutput left, CompileOutput right) => !Equals(left, right);

    public override string ToString()
      => $"CompileOutput(AssemblyPath: {AssemblyPath}, Timestamp: {Timestamp})";

    public static InOut ToInOut(CompileOutput compileOutput) => InOut.Create(compileOutput.AssemblyPath, compileOutput.Success);
  }
}