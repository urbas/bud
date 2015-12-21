using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bud.IO;
using Bud.V1;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using static Bud.V1.Api;

namespace Bud.Cs {
  public class TimedEmittingCompiler {
    public string OutputAssemblyPath { get; }
    public IImmutableList<ResourceDescription> EmbeddedResources { get; }
    public ICompiler UnderlyingCompiler { get; }
    public Stopwatch Stopwatch { get; } = new Stopwatch();

    public TimedEmittingCompiler(IImmutableList<ResourceDescription> embeddedResources,
                                 ICompiler underlyingCompiler,
                                 string outputAssemblyPath) {
      OutputAssemblyPath = outputAssemblyPath;
      EmbeddedResources = embeddedResources;
      UnderlyingCompiler = underlyingCompiler;
    }

    public static Func<IEnumerable<object>, CompileOutput> Create(IConf conf)
      => new TimedEmittingCompiler(Api.EmbeddedResources[conf],
                                   CreateUnderlyingCompiler(conf),
                                   GetOutputAssemblyPath(conf)).Compile;

    public CompileOutput Compile(IEnumerable<object> inOutInput) {
      List<Timestamped<string>> sources;
      List<Timestamped<string>> assemblies;
      List<CompileOutput> dependencies;
      CompileInput.ExtractInput(inOutInput, out sources, out assemblies, out dependencies);

      if (!dependencies.All(dependency => dependency.Success)) {
        return CreateOutputFromAssembly(false);
      }

      if (File.Exists(OutputAssemblyPath) && IsOutputUpToDate(sources, assemblies)) {
        return CreateOutputFromAssembly(true);
      }

      Stopwatch.Restart();
      var cSharpCompilation = UnderlyingCompiler.Compile(sources, assemblies);

      if (cSharpCompilation == null) {
        throw new Exception("Unexpected compiler error.");
      }

      return EmitDll(cSharpCompilation, Stopwatch, EmbeddedResources, OutputAssemblyPath);
    }

    private CompileOutput CreateOutputFromAssembly(bool isSuccess)
      => new CompileOutput(Enumerable.Empty<Diagnostic>(),
                           Stopwatch.Elapsed,
                           OutputAssemblyPath,
                           isSuccess,
                           Files.GetFileTimestamp(OutputAssemblyPath),
                           MetadataReference.CreateFromFile(OutputAssemblyPath));

    private bool IsOutputUpToDate(IEnumerable<Timestamped<string>> sources,
                                  IEnumerable<Timestamped<string>> assemblies) {
      var timestampedFile = Files.ToTimestampedFile(OutputAssemblyPath);
      return timestampedFile.IsUpToDateWith(sources) &&
             timestampedFile.IsUpToDateWith(assemblies);
    }

    private static CompileOutput EmitDll(Compilation compilation,
                                         Stopwatch stopwatch,
                                         IEnumerable<ResourceDescription> embeddedResources,
                                         string outputAssemblyPath) {
      Directory.CreateDirectory(Path.GetDirectoryName(outputAssemblyPath));
      EmitResult emitResult;
      using (var assemblyOutputFile = File.Create(outputAssemblyPath)) {
        emitResult = compilation.Emit(assemblyOutputFile, manifestResources: embeddedResources);
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

    private static RoslynCSharpCompiler CreateUnderlyingCompiler(IConf conf)
      => new RoslynCSharpCompiler(AssemblyName[conf], CSharpCompilationOptions[conf]);

    private static string GetOutputAssemblyPath(IConf conf)
      => Path.Combine(TargetDir[conf], AssemblyName[conf]);
  }
}