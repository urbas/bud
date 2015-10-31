using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Bud.Compilation {
  public class CSharpCompilationOutput {
    public MetadataReference Reference { get; }
    public long Timestamp { get; }
    public IEnumerable<Diagnostic> Diagnostics { get; }
    public TimeSpan CompilationTime { get; }
    public string AssemblyPath { get; }
    public bool Success { get; }

    public CSharpCompilationOutput(IEnumerable<Diagnostic> diagnostics,
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

    public AssemblyReference ToAssemblyReference()
      => new AssemblyReference(AssemblyPath, Reference);

    protected bool Equals(CSharpCompilationOutput other)
      => Timestamp == other.Timestamp && string.Equals(AssemblyPath, other.AssemblyPath);

    public override bool Equals(object obj)
      => !ReferenceEquals(null, obj) &&
         (ReferenceEquals(this, obj) ||
          obj.GetType() == GetType() &&
          Equals((CSharpCompilationOutput) obj));

    public override int GetHashCode() {
      unchecked {
        return (Timestamp.GetHashCode() * 397) ^ AssemblyPath.GetHashCode();
      }
    }

    public static bool operator ==(CSharpCompilationOutput left, CSharpCompilationOutput right) => Equals(left, right);
    public static bool operator !=(CSharpCompilationOutput left, CSharpCompilationOutput right) => !Equals(left, right);
  }
}