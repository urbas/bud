using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Bud.Compilation {
  public class TimedEmittingCompiler {
    public string OutputAssemblyPath { get; }
    public IConf Conf { get; }
    public Func<CSharpCompilationInput, CSharpCompilation> UnderlyingCompiler { get; }
    public Stopwatch Stopwatch { get; } = new Stopwatch();

    public TimedEmittingCompiler(IConf conf) {
      OutputAssemblyPath = GetOutputAssemblyPath(conf);
      var assemblyName = CSharp.AssemblyName[conf];
      var cSharpCompilationOptions = CSharp.CSharpCompilationOptions[conf];
      UnderlyingCompiler = RoslynCSharpCompiler.Create(assemblyName, cSharpCompilationOptions);
      Conf = conf;
    }

    public static Func<CSharpCompilationInput, CSharpCompilationOutput> Create(IConf conf)
      => new TimedEmittingCompiler(conf).Compile;

    public CSharpCompilationOutput Compile(CSharpCompilationInput compilationInput) {
      Stopwatch.Restart();
      if (IsOutputUpToDate(compilationInput)) {
        return CreateOutputFromAssembly();
      }
      return EmitDllAndPrintResult(UnderlyingCompiler(compilationInput), Stopwatch);
    }

    private static string GetOutputAssemblyPath(IConf conf)
      => Path.Combine(CSharp.OutputDir[conf], CSharp.AssemblyName[conf]);

    private CSharpCompilationOutput CreateOutputFromAssembly()
      => new CSharpCompilationOutput(Enumerable.Empty<Diagnostic>(),
                                     Stopwatch.Elapsed,
                                     OutputAssemblyPath,
                                     true,
                                     Files.GetTimeHash(OutputAssemblyPath),
                                     MetadataReference.CreateFromFile(OutputAssemblyPath));

    private bool IsOutputUpToDate(CSharpCompilationInput compilationInput)
      => File.Exists(OutputAssemblyPath) &&
         IsFileUpToDate(OutputAssemblyPath, compilationInput.Sources) &&
         IsFileUpToDate(OutputAssemblyPath, compilationInput.Assemblies);

    private static bool IsFileUpToDate<T>(string file, IEnumerable<Hashed<T>> otherResources)
      => !otherResources.All(hashed => Files.TimeHashEquals(hashed, file));

    private CSharpCompilationOutput EmitDllAndPrintResult(CSharpCompilation compilation, Stopwatch stopwatch) {
      Directory.CreateDirectory(Path.GetDirectoryName(OutputAssemblyPath));
      EmitResult emitResult;
      using (var assemblyOutputFile = File.Create(OutputAssemblyPath)) {
        emitResult = compilation.Emit(assemblyOutputFile);
        stopwatch.Stop();
      }
      if (!emitResult.Success) {
        File.Delete(OutputAssemblyPath);
      }
      return new CSharpCompilationOutput(emitResult.Diagnostics,
                                         stopwatch.Elapsed,
                                         OutputAssemblyPath,
                                         emitResult.Success,
                                         Files.GetTimeHash(),
                                         compilation.ToMetadataReference());
    }
  }
}