using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.NuGet;
using Bud.Reactive;
using Bud.Util;
using Bud.V1;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static System.Console;
using static System.IO.Path;
using static Bud.NuGet.NuGetPublishing;
using static Bud.V1.Api;
using static Microsoft.CodeAnalysis.OutputKind;

namespace Bud.Cs {
  internal static class CsProjects {
    private static readonly Conf CsProjectSetting = Conf
      .Empty
      .Add(NuGetPublishingSupport)
      .AddSources(fileFilter: "*.cs")
      .Init(Compile, DefaultCSharpCompilation)
      .Add(Build, c => Compile[c].Select(output => output.AssemblyPath))
      .Init(AssemblyName, c => ProjectId[c] + CsCompilationOptions[c].OutputKind.ToExtension())
      .InitEmpty(AssemblyReferences)
      .InitEmpty(EmbeddedResources)
      .Init(Compiler, TimedEmittingCompiler.Create)
      .InitValue(CsCompilationOptions,
                 new CSharpCompilationOptions(DynamicallyLinkedLibrary,
                                              warningLevel: 1))
      .Add(AssemblyReferences, c => ("NuGetPackageReference"/ResolvedAssemblies)[c])
      .Set("NuGetPackageReference"/ProjectDir, c => Combine(ProjectDir[c], "packages"))
      .Set("NuGetPackageReference"/PackagesConfigFile, c => Combine(ProjectDir[c], "packages.config"))
      .Set(PackageFiles, PackageLibDlls)
      .ExcludeSourceDirs(DefaultExcludedSourceDirs);

    private static IObservable<IEnumerable<PackageFile>> PackageLibDlls(IConf c)
      => Output[c].Select(ToPackageFiles);

    internal static Conf CreateCsLibrary(string projectDir, string projectId)
      => BuildProject(projectDir, projectId)
        .Add(PackageReferencesProject(projectDir, "NuGetPackageReference"))
        .Add(CsProjectSetting);

    internal static IObservable<CompileOutput> DefaultCSharpCompilation(IConf conf)
      => Input[conf]
        .CombineLatest(ObserveDependencies(conf),
                       AssemblyReferences[conf], CompileInput.Create)
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
      => new ResourceDescription(nameInAssembly, () => File.OpenRead(resourceFile), true);

    internal static IObservable<IEnumerable<CompileOutput>> ObserveDependencies(IConf c)
      => Dependencies[c].Gather(dependency => c.TryGet(dependency/Compile))
                        .Combined();

    internal static Conf EmbedResourceImpl(Conf conf, string path, string nameInAssembly)
      => conf.Add(EmbeddedResources, c => {
        var resourceFile = IsPathRooted(path) ? path : Combine(ProjectDir[c], path);
        return ToResourceDescriptor(resourceFile, nameInAssembly);
      });

    private static IEnumerable<PackageFile> ToPackageFiles(IEnumerable<string> dlls)
      => dlls.Select(dll => new PackageFile(dll, $"lib/{GetFileName(dll)}"));

    private static IEnumerable<string> DefaultExcludedSourceDirs(IConf c)
      => new[] {
        "obj",
        "bin",
        BudDir[c],
        ("NuGetPackageReference"/ProjectDir)[c]
      };
  }
}