using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bud.IO;
using Bud.V1;
using Microsoft.CodeAnalysis;
using static System.IO.File;
using static Bud.IO.FileUtils;
using static Bud.V1.Api;

namespace Bud.Cs {
  public class TimedEmittingCompiler {
    public string OutputAssemblyPath { get; }
    public ICompiler UnderlyingCompiler { get; }
    private Stopwatch Stopwatch { get; } = new Stopwatch();

    public TimedEmittingCompiler(ICompiler underlyingCompiler,
                                 string outputAssemblyPath) {
      OutputAssemblyPath = outputAssemblyPath;
      UnderlyingCompiler = underlyingCompiler;
    }

    public static Func<CompileInput, CompileOutput> Create(IConf conf)
      => new TimedEmittingCompiler(
        new RoslynCsCompiler(EmbeddedResources[conf],
                             AssemblyName[conf],
                             CsCompilationOptions[conf]),
        GetOutputAssemblyPath(conf))
        .Compile;

    public CompileOutput Compile(CompileInput input) {
      Stopwatch.Restart();

      if (!input.Dependencies.All(dependency => dependency.Success)) {
        return CreateOutputFromAssembly(false,
                                        Exists(OutputAssemblyPath) ?
                                          MetadataReference.CreateFromFile(OutputAssemblyPath) :
                                          null);
      }

      var sources = input.Sources.Select(ToTimestampedFile).ToList();
      var assemblies = input.AssemblyReferences
                            .Concat(input.Dependencies.Select(output => output.AssemblyPath))
                            .Select(ToTimestampedFile).ToList();

      if (Exists(OutputAssemblyPath) && IsOutputUpToDate(sources, assemblies)) {
        return CreateOutputFromAssembly(
          true,
          MetadataReference.CreateFromFile(OutputAssemblyPath));
      }

      return UnderlyingCompiler.Compile(sources,
                                        assemblies,
                                        OutputAssemblyPath);
    }

    private CompileOutput CreateOutputFromAssembly(
      bool isSuccess,
      MetadataReference outputAssembly)
      => new CompileOutput(Enumerable.Empty<Diagnostic>(),
                           Stopwatch.Elapsed,
                           OutputAssemblyPath,
                           isSuccess,
                           GetFileTimestamp(OutputAssemblyPath),
                           outputAssembly);

    private bool IsOutputUpToDate(IEnumerable<Timestamped<string>> sources,
                                  IEnumerable<Timestamped<string>> assemblies) {
      var timestampedFile = ToTimestampedFile(OutputAssemblyPath);
      return timestampedFile.IsUpToDateWith(sources) &&
             timestampedFile.IsUpToDateWith(assemblies);
    }

    private static string GetOutputAssemblyPath(IConf conf)
      => Path.Combine(BudDir[conf], AssemblyName[conf]);
  }
}