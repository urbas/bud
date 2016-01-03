using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Bud.Cs;
using Bud.Reactive;
using Bud.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static System.Console;
using static System.IO.File;
using static System.IO.Path;
using static Bud.V1.Api;

namespace Bud.V1 {
  internal static class CsProjects {
    internal static Conf CreateCsLibrary(string projectDir, string projectId)
      => BuildProject(projectDir, projectId)
        .AddSources(fileFilter: "*.cs")
        .ExcludeSourceDirs("obj", "bin", TargetDirName)
        .Init(Compile, DefaultCSharpCompilation)
        .Add(Build, c => Compile[c].Select(output => output.AssemblyPath))
        .Init(AssemblyName, c => ProjectId[c] + CsCompilationOptions[c].OutputKind.ToExtension())
        .InitEmpty(AssemblyReferences)
        .InitEmpty(EmbeddedResources)
        .Init(Compiler, TimedEmittingCompiler.Create)
        .InitValue(CsCompilationOptions,
                   new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                                                warningLevel: 1));

    internal static IObservable<CompileOutput> DefaultCSharpCompilation(IConf conf)
      => Input[conf]
        .CombineLatest(ObserveDependencies(conf),
                       (sources, dependencies) => new CompileInput(
                                                    sources,
                                                    dependencies,
                                                    AssemblyReferences[conf]
                                                    ))
        .Select(Compiler[conf]).Do(PrintCompilationResult);

    internal static void PrintCompilationResult(CompileOutput output) {
      if (output.Success) {
        WriteLine($"Compiled '{GetFileNameWithoutExtension(output.AssemblyPath)}' in {output.CompilationTime.Milliseconds}ms.");
      } else {
        WriteLine($"Failed to compile '{output.AssemblyPath}'.");
        foreach (var diagnostic in output.Diagnostics) {
          WriteLine(diagnostic);
        }
      }
    }

    internal static ResourceDescription ToResourceDescriptor(string resourceFile, string nameInAssembly)
      => new ResourceDescription(nameInAssembly, () => OpenRead(resourceFile), true);

    internal static IObservable<IEnumerable<CompileOutput>> ObserveDependencies(IConf c)
      => Dependencies[c].Gather(dependency => c.TryGet(dependency/Compile))
                        .Combined();

    internal static Conf EmbedResourceImpl(Conf conf, string path, string nameInAssembly)
      => conf.Add(EmbeddedResources, c => {
        var resourceFile = IsPathRooted(path) ? path : Combine(ProjectDir[c], path);
        return ToResourceDescriptor(resourceFile, nameInAssembly);
      });
  }
}