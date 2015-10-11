using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using Bud.Pipeline;

namespace Bud.Compilation {
  public class TimedEmittingCompiler {
    public IConfigs Configs { get; }
    public Pipe<CompilationInput, CompilationOutput> UnderlyingCompiler { get; }

    public TimedEmittingCompiler(Pipe<CompilationInput, CompilationOutput> underlyingCompiler, IConfigs configs) {
      UnderlyingCompiler = underlyingCompiler;
      Configs = configs;
    }

    public static Pipe<CompilationInput, CompilationOutput> Create(IConfigs configs, Pipe<CompilationInput, CompilationOutput> oldCompiler)
      => new TimedEmittingCompiler(oldCompiler, configs).Compile;

    public IObservable<CompilationOutput> Compile(IObservable<CompilationInput> inputPipe) {
      var stopwatch = new Stopwatch();
      return inputPipe.Do(_ => stopwatch.Restart())
                      .ApplyPipe(UnderlyingCompiler)
                      .Do(output => EmitDllAndPrintResult(output, stopwatch, Configs));
    }

    private static void EmitDllAndPrintResult(CompilationOutput compilationOutput, Stopwatch stopwatch, IConfigs configs) {
      var assemblyPath = Path.Combine(CSharp.OutputDir[configs], CSharp.AssemblyName[configs]);
      Directory.CreateDirectory(CSharp.OutputDir[configs]);
      using (var assemblyOutputFile = File.Create(assemblyPath)) {
        var emitResult = compilationOutput.Compilation.Emit(assemblyOutputFile);
        stopwatch.Stop();
        Console.WriteLine($"Compiled: {assemblyPath}, Success: {emitResult.Success}, Time: {stopwatch.ElapsedMilliseconds}ms");
      }
    }
  }
}