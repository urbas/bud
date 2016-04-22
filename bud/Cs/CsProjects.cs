using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
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
    private const string PackagesSubProjectId = "Packages";

    private static readonly Conf CsProjectSetting = Conf
      .Empty
      .Add(NuGetPublishingSupport)
      .AddSources(fileFilter: "*.cs")
      .Init(Compile, DefaultCSharpCompilation)
      .Add(Build, DefaultBuild)
      .Init(AssemblyName, DefaultAssemblyName)
      .InitEmpty(AssemblyReferences)
      .InitEmpty(EmbeddedResources)
      .Init(Compiler, TimedEmittingCompiler.Create)
      .Init(CsCompilationOptions,
                 new CSharpCompilationOptions(DynamicallyLinkedLibrary,
                                              warningLevel: 1))
      .Add(AssemblyReferences, c => (PackagesSubProjectId/ResolvedAssemblies)[c])
      .Add(AssemblyReferences, WindowsFrameworkAssemblyResolver.ResolveFrameworkAssembly("mscorlib", new Version(0, 0)).Value)
      .Set(PackagesSubProjectId/ProjectDir, c => Combine(ProjectDir[c], "packages"))
      .Set(PackagesSubProjectId/PackagesConfigFile, c => Combine(ProjectDir[c], "packages.config"))
      .Init(ReferencedPackages, c => (PackagesSubProjectId/ReferencedPackages)[c])
      .Set(PackageFiles, PackageLibDlls)
      .Add(FilesToDistribute, AssembliesPackagedPaths)
      .ExcludeSourceDirs(DefaultExcludedSourceDirs);

    private static IObservable<IImmutableList<PackageFile>> AssembliesPackagedPaths(IConf c)
      => AssemblyReferences[c].Select(PackageAssemblies)
                              .Select(ImmutableList.ToImmutableList);

    private static IEnumerable<PackageFile> PackageAssemblies(IImmutableList<string> files)
      => from assemblyPath in files
         let assemblyFileName = GetFileName(assemblyPath)
         where !WindowsFrameworkAssemblyResolver.IsFrameworkAssembly(assemblyFileName)
         select new PackageFile(assemblyPath, assemblyFileName);

    internal static Conf CsLib(string projectDir, string projectId)
      => BuildProject(projectDir, projectId)
        .Add(PackageReferencesProject(projectDir, PackagesSubProjectId))
        .Add(CsProjectSetting);

    internal static Conf CsApp(string projectDir, string projectId)
      => CsLib(projectDir, projectId)
        .Modify(CsCompilationOptions, (_, oldValue) => oldValue.WithOutputKind(ConsoleApplication));

    internal static Conf EmbedResourceImpl(Conf conf, string path, string nameInAssembly)
      => conf.Add(EmbeddedResources, c => {
        var resourceFile = IsPathRooted(path) ? path : Combine(ProjectDir[c], path);
        return ToResourceDescriptor(resourceFile, nameInAssembly);
      });

    private static IObservable<string> DefaultBuild(IConf c)
      => Compile[c].Select(output => output.AssemblyPath);

    private static string DefaultAssemblyName(IConf c)
      => ProjectId[c] + CsCompilationOptions[c].OutputKind.ToExtension();

    private static IObservable<CompileOutput> DefaultCSharpCompilation(IConf conf)
      => Input[conf]
        .CombineLatest(ObserveDependencies(conf),
                       AssemblyReferences[conf],
                       CompileInput.Create)
        .Select(Compiler[conf]).Do(PrintCompilationResult);

    private static void PrintCompilationResult(CompileOutput output) {
      if (output.Success) {
        WriteLine($"Compiled '{GetFileNameWithoutExtension(output.AssemblyPath)}' in {output.CompilationTime.Milliseconds}ms.");
      } else {
        WriteLine($"Failed to compile '{output.AssemblyPath}'.");
        foreach (var diagnostic in output.Diagnostics) {
          WriteLine(diagnostic);
        }
      }
    }

    private static ResourceDescription ToResourceDescriptor(string resourceFile, string nameInAssembly)
      => new ResourceDescription(nameInAssembly, () => File.OpenRead(resourceFile), true);

    private static IObservable<IEnumerable<CompileOutput>> ObserveDependencies(IConf c)
      => Dependencies[c].Gather(dependency => c.TryGet(dependency/Compile))
                        .Combined();

    private static IEnumerable<PackageFile> ToPackageFiles(IEnumerable<string> dlls)
      => dlls.Select(dll => new PackageFile(dll, $"lib/{GetFileName(dll)}"));

    private static IObservable<IEnumerable<PackageFile>> PackageLibDlls(IConf c)
      => Output[c].Select(ToPackageFiles);

    private static IEnumerable<string> DefaultExcludedSourceDirs(IConf c)
      => new[] {
        "obj",
        "bin",
        BuildDir[c],
        (PackagesSubProjectId/ProjectDir)[c]
      };
  }
}