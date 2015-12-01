using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;

namespace Bud.Cs {
  public class TimedEmittingCompiler {
    public string OutputAssemblyPath { get; }
    public IConf Conf { get; }
    public RoslynCSharpCompiler UnderlyingCompiler { get; }
    public Stopwatch Stopwatch { get; } = new Stopwatch();

    public TimedEmittingCompiler(IConf conf) {
      OutputAssemblyPath = GetOutputAssemblyPath(conf);
      var assemblyName = CSharp.AssemblyName[conf];
      var cSharpCompilationOptions = CSharp.CSharpCompilationOptions[conf];
      UnderlyingCompiler = new RoslynCSharpCompiler(assemblyName, cSharpCompilationOptions);
      Conf = conf;
    }

    public static Func<InOut, CompileOutput> Create(IConf conf)
      => new TimedEmittingCompiler(conf).Compile;

    public CompileOutput Compile(InOut inOutInput) {
      Stopwatch.Restart();

      var input = CompileInput.FromInOut(inOutInput);
      var sources = Files.ToTimestampedFiles(input.Sources).ToList();
      var assemblies = Files.ToTimestampedFiles(input.Assemblies).ToList();

      if (File.Exists(OutputAssemblyPath) && IsOutputUpToDate(sources, assemblies)) {
        return CreateOutputFromAssembly();
      }

      var cSharpCompilation = UnderlyingCompiler.Compile(sources, assemblies);
      return EmitDll(cSharpCompilation, Stopwatch, CSharp.EmbeddedResources[Conf], OutputAssemblyPath);
    }

    private static string GetOutputAssemblyPath(IConf conf)
      => Path.Combine(CSharp.OutputDir[conf], CSharp.AssemblyName[conf]);

    private CompileOutput CreateOutputFromAssembly()
      => new CompileOutput(Enumerable.Empty<Diagnostic>(),
                           Stopwatch.Elapsed,
                           OutputAssemblyPath,
                           true,
                           Files.GetFileTimestamp(OutputAssemblyPath),
                           MetadataReference.CreateFromFile(OutputAssemblyPath));

    private bool IsOutputUpToDate(IEnumerable<Timestamped<string>> sources, IEnumerable<Timestamped<string>> assemblies) {
      var timestampedFile = Files.ToTimestampedFile(OutputAssemblyPath);
      return timestampedFile.IsUpToDateWith(sources) &&
             timestampedFile.IsUpToDateWith(assemblies);
    }

    private static CompileOutput EmitDll(Compilation compilation, Stopwatch stopwatch, IEnumerable<ResourceDescription> manifestResources, string outputAssemblyPath) {
      Directory.CreateDirectory(Path.GetDirectoryName(outputAssemblyPath));
      EmitResult emitResult;
      using (var assemblyOutputFile = File.Create(outputAssemblyPath)) {
        emitResult = compilation.Emit(assemblyOutputFile, manifestResources: manifestResources);
        stopwatch.Stop();
      }
      if (!emitResult.Success) {
        File.Delete(outputAssemblyPath);
      }
      return new CompileOutput(emitResult.Diagnostics,
                               stopwatch.Elapsed,
                               outputAssemblyPath,
                               emitResult.Success,
                               Files.FileTimestampNow(),
                               compilation.ToMetadataReference());
    }
  }
}