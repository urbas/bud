using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Bud.Compilation {
  public class TimedEmittingCompiler {
    public string OutputAssemblyPath { get; }
    public IConfigs Configs { get; }
    public Func<CompilationInput, CSharpCompilation> UnderlyingCompiler { get; }
    public Stopwatch Stopwatch { get; } = new Stopwatch();

    public TimedEmittingCompiler(Func<CompilationInput, CSharpCompilation> underlyingCompiler, IConfigs configs, string outputAssemblyPath) {
      OutputAssemblyPath = outputAssemblyPath;
      UnderlyingCompiler = underlyingCompiler;
      Configs = configs;
    }

    public static Func<CompilationInput, CompilationOutput> Create(IConfigs configs)
      => new TimedEmittingCompiler(new RoslynCSharpCompiler(configs).Compile, configs, Path.Combine(CSharp.OutputDir[configs], CSharp.AssemblyName[configs])).Compile;

    public CompilationOutput Compile(CompilationInput compilationInput) {
      Stopwatch.Restart();
      if (File.Exists(OutputAssemblyPath) && IsFileUpToDate(OutputAssemblyPath, compilationInput.Sources) && IsFileUpToDate(OutputAssemblyPath, compilationInput.Dependencies)) {
        return new CompilationOutput(Enumerable.Empty<Diagnostic>(), Stopwatch.Elapsed, OutputAssemblyPath, true, File.GetLastWriteTime(OutputAssemblyPath), MetadataReference.CreateFromFile(OutputAssemblyPath));
      }
      return EmitDllAndPrintResult(UnderlyingCompiler(compilationInput), Stopwatch);
    }

    private static bool IsFileUpToDate<T>(string file, IEnumerable<Timestamped<T>> otherResources)
      => otherResources.Any() && File.GetLastWriteTime(file) >= otherResources.Select(timestamped => timestamped.Timestamp).Max();

    private static bool IsFileUpToDate(string file, IEnumerable<string> otherFiles)
      => otherFiles.Any() && File.GetLastWriteTime(file) >= otherFiles.Select(File.GetLastWriteTime).Max();

    private CompilationOutput EmitDllAndPrintResult(CSharpCompilation compilation, Stopwatch stopwatch) {
      Directory.CreateDirectory(Path.GetDirectoryName(OutputAssemblyPath));
      EmitResult emitResult;
      using (var assemblyOutputFile = File.Create(OutputAssemblyPath)) {
        emitResult = compilation.Emit(assemblyOutputFile);
        stopwatch.Stop();
      }
      return new CompilationOutput(emitResult.Diagnostics, stopwatch.Elapsed, OutputAssemblyPath, emitResult.Success, DateTime.Now, compilation.ToMetadataReference());
    }
  }
}