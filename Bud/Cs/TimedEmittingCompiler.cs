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
      if (IsOutputUpToDate(compilationInput)) {
        return CreateOutputFromAssembly();
      }
      return EmitDllAndPrintResult(UnderlyingCompiler(compilationInput), Stopwatch);
    }

    private static string GetOutputAssemblyPath(IConf conf)
      => Path.Combine(CSharp.OutputDir[conf], CSharp.AssemblyName[conf]);

    private CompileOutput CreateOutputFromAssembly()
      => new CompileOutput(Enumerable.Empty<Diagnostic>(),
                                     Stopwatch.Elapsed,
                                     OutputAssemblyPath,
                                     true,
                                     Files.GetTimeHash(OutputAssemblyPath),
                                     MetadataReference.CreateFromFile(OutputAssemblyPath));

    private bool IsOutputUpToDate(CompileInput compilationInput)
      => File.Exists(OutputAssemblyPath) &&
         IsFileUpToDate(OutputAssemblyPath, compilationInput.Sources) &&
         IsFileUpToDate(OutputAssemblyPath, compilationInput.Assemblies);

    private static bool IsFileUpToDate<T>(string file, IEnumerable<Hashed<T>> otherResources)
      => !otherResources.All(hashed => Files.TimeHashEquals(hashed, file));

    private CompileOutput EmitDllAndPrintResult(CSharpCompilation compilation, Stopwatch stopwatch) {
      Directory.CreateDirectory(Path.GetDirectoryName(OutputAssemblyPath));
      EmitResult emitResult;
      using (var assemblyOutputFile = File.Create(OutputAssemblyPath)) {
        emitResult = compilation.Emit(assemblyOutputFile);
        stopwatch.Stop();
      }
      if (!emitResult.Success) {
        File.Delete(OutputAssemblyPath);
      }
      return new CompileOutput(emitResult.Diagnostics,
                                         stopwatch.Elapsed,
                                         OutputAssemblyPath,
                                         emitResult.Success,
                                         Files.GetTimeHash(),
                                         compilation.ToMetadataReference());
    }
  }
}