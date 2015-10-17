using System;
using System.Collections.Generic;
using System.Reactive;
using Bud.IO;
using Microsoft.CodeAnalysis;

namespace Bud.Compilation {
  public class CompilationOutput {
    public MetadataReference Reference { get; }
    public long Timestamp { get; }
    public IEnumerable<Diagnostic> Diagnostics { get; }
    public TimeSpan CompilationTime { get; }
    public string AssemblyPath { get; }
    public bool Success { get; }

    public CompilationOutput(IEnumerable<Diagnostic> diagnostics, TimeSpan compilationTime, string assemblyPath, bool success, long timestamp, MetadataReference reference) {
      Diagnostics = diagnostics;
      CompilationTime = compilationTime;
      AssemblyPath = assemblyPath;
      Success = success;
      Timestamp = timestamp;
      Reference = reference;
    }

    public IO.Hashed<Dependency> ToTimestampedDependency()
      => new IO.Hashed<Dependency>(new Dependency(AssemblyPath, Reference), Timestamp);
  }
}