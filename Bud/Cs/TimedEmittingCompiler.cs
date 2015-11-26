using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Bud.Cs {
  public class TimedEmittingCompiler {
    public string OutputAssemblyPath { get; }
    public IConf Conf { get; }
    public Func<CompileInput, CSharpCompilation> UnderlyingCompiler { get; }
    public Stopwatch Stopwatch { get; } = new Stopwatch();

    public TimedEmittingCompiler(IConf conf) {
      OutputAssemblyPath = GetOutputAssemblyPath(conf);
      var assemblyName = CSharp.AssemblyName[conf];
      var cSharpCompilationOptions = CSharp.CSharpCompilationOptions[conf];
      UnderlyingCompiler = RoslynCSharpCompiler.Create(assemblyName, cSharpCompilationOptions);
      Conf = conf;
    }

    public static Func<CompileInput, CompileOutput> Create(IConf conf)
      => new TimedEmittingCompiler(conf).Compile;

    public CompileOutput Compile(CompileInput compilationInput) {
      Stopwatch.Restart();
      if (File.Exists(OutputAssemblyPath) && IsOutputUpToDate(compilationInput)) {
        return CreateOutputFromAssembly();
      }
      return EmitDll(UnderlyingCompiler(compilationInput), Stopwatch, CSharp.EmbeddedResources[Conf]);
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

    private bool IsOutputUpToDate(CompileInput compilationInput) {
      var timestampedFile = Files.ToTimestampedFile(OutputAssemblyPath);
      return timestampedFile.IsUpToDateWith(compilationInput.Sources) &&
             timestampedFile.IsUpToDateWith(compilationInput.Assemblies);
    }

    private CompileOutput EmitDll(CSharpCompilation compilation, Stopwatch stopwatch, IEnumerable<ResourceDescription> manifestResources) {
      Directory.CreateDirectory(Path.GetDirectoryName(OutputAssemblyPath));
      EmitResult emitResult;
      using (var assemblyOutputFile = File.Create(OutputAssemblyPath)) {
        emitResult = compilation.Emit(assemblyOutputFile, manifestResources: manifestResources);
        stopwatch.Stop();
      }
      if (!emitResult.Success) {
        File.Delete(OutputAssemblyPath);
      }
      return new CompileOutput(emitResult.Diagnostics,
                               stopwatch.Elapsed,
                               OutputAssemblyPath,
                               emitResult.Success,
                               Files.FileTimestampNow(),
                               compilation.ToMetadataReference());
    }
  }
}