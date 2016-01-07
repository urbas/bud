using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using static System.IO.Directory;
using static System.IO.Path;

namespace Bud.Cs {
  public class RoslynDllEmitter : ICompiler {
    private readonly RoslynCSharpCompiler compiler;
    private readonly IEnumerable<ResourceDescription> embeddedResources;

    public RoslynDllEmitter(IEnumerable<ResourceDescription> embeddedResources,
                            string assemblyName,
                            CSharpCompilationOptions options) {
      compiler = new RoslynCSharpCompiler(assemblyName, options);
      this.embeddedResources = embeddedResources;
    }

    public CompileOutput Compile(IEnumerable<Timestamped<string>> sources,
                                 IEnumerable<Timestamped<string>> assemblies,
                                 string outputAssemblyPath,
                                 Stopwatch stopwatch) {
      var cSharpCompilation = compiler.Compile(sources, assemblies);

      if (cSharpCompilation == null) {
        throw new Exception("Unexpected compiler error.");
      }

      return EmitDll(cSharpCompilation, stopwatch, embeddedResources, outputAssemblyPath);
    }

    private static CompileOutput EmitDll(Compilation compilation,
                                         Stopwatch stopwatch,
                                         IEnumerable<ResourceDescription> embeddedResources,
                                         string outputAssemblyPath) {
      CreateDirectory(GetDirectoryName(outputAssemblyPath));
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
                               FileUtils.FileTimestampNow(),
                               compilation.ToMetadataReference());
    }
  }
}