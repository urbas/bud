using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.Cs;
using Bud.IO;
using Bud.NuGet;
using Bud.Reactive;
using Bud.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Bud.V1.Api;
using static Bud.V1.Basic;
using static Bud.V1.Builds;

namespace Bud.V1 {
  public static class Cs {
    private const string PackagesSubProjectId = "Packages";

    public static readonly Key<IObservable<CompileOutput>> Compile = nameof(Compile);
    public static readonly Key<Func<CompileInput, CompileOutput>> Compiler = nameof(Compiler);
    public static readonly Key<IObservable<IImmutableList<string>>> AssemblyReferences = nameof(AssemblyReferences);

    /// <summary>
    ///   The name of the assembly to be built (with the extension).
    /// </summary>
    public static readonly Key<string> AssemblyName = nameof(AssemblyName);

    public static readonly Key<CSharpCompilationOptions> CsCompilationOptions = nameof(CsCompilationOptions);
    public static readonly Key<IImmutableList<ResourceDescription>> EmbeddedResources = nameof(EmbeddedResources);

    private static readonly Conf CsProjectSetting = NuGetPublishing
      .NuGetPublishingSupportImpl
      .AddSources(fileFilter: "*.cs")
      .Init(Compile, DefaultCSharpCompilation)
      .Add(Build, DefaultBuild)
      .Init(AssemblyName, DefaultAssemblyName)
      .InitEmpty(AssemblyReferences)
      .InitEmpty(EmbeddedResources)
      .Init(Compiler, TimedEmittingCompiler.Create)
      .Init(CsCompilationOptions,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                                         warningLevel: 1))
      .Add(AssemblyReferences, c => (PackagesSubProjectId/ResolvedAssemblies)[c])
      .Add(AssemblyReferences, WindowsFrameworkAssemblyResolver.ResolveFrameworkAssembly("mscorlib", new Version(0, 0)).Value)
      .Set(PackagesSubProjectId/ProjectDir, c => Path.Combine(ProjectDir[c], "packages"))
      .Set(PackagesSubProjectId/PackagesConfigFile, c => Path.Combine(ProjectDir[c], "packages.config"))
      .Init(ReferencedPackages, c => (PackagesSubProjectId/ReferencedPackages)[c])
      .Set(NuGetPublishing.PackageFiles, PackageLibDlls)
      .Add(FilesToDistribute, AssembliesPackagedPaths)
      .ExcludeSourceDirs(DefaultExcludedSourceDirs);

    /// <summary>
    ///   Configures a C# library project named <paramref name="projectId" /> and placed in the
    ///   directory with the same name. The project's directory will be placed  in the current
    ///   working directory.
    /// </summary>
    /// <param name="projectId">see <see cref="Basic.ProjectId" />.</param>
    /// <param name="projectDir">
    ///   This is the directory in which all sources of this project will live.
    ///   <para>
    ///     If none given, the <see cref="Basic.ProjectDir" /> will be <see cref="Basic.BaseDir" /> appended with the
    ///     <see cref="Basic.ProjectId" />.
    ///   </para>
    ///   <para>
    ///     If the given path is relative, then the absolute <see cref="Basic.ProjectDir" /> will
    ///     be resolved from the <see cref="Basic.BaseDir" />. Note that the <paramref name="projectDir" />
    ///     can be empty.
    ///   </para>
    ///   <para>
    ///     If the given path is absolute, the absolute path will be taken verbatim.
    ///   </para>
    /// </param>
    /// <param name="baseDir">
    ///   <para>
    ///     The directory under which all projects should live. By default this is the directory
    ///     where the <c>Build.cs</c> script is located.
    ///   </para>
    ///   <para>
    ///     By default this is where the <see cref="Basic.BuildDir" /> will be located.
    ///   </para>
    /// </param>
    public static Conf CsLib(string projectId,
                             Option<string> projectDir = default(Option<string>),
                             Option<string> baseDir = default(Option<string>))
      => BuildProject(projectId, projectDir, baseDir)
               .Add(PackageReferencesProject(PackagesSubProjectId, projectDir, baseDir))
               .Add(CsProjectSetting);

    /// <summary>
    ///   Similar to <see cref="CsLib" /> but produces a console application instead
    ///   of a library.
    /// </summary>
    /// <param name="projectId">see <see cref="Basic.ProjectId" />.</param>
    /// <param name="projectDir">
    ///   This is the directory in which all sources of this project will live.
    ///   <para>
    ///     If none given, the <see cref="Basic.ProjectDir" /> will be <see cref="Basic.BaseDir" /> appended with the
    ///     <see cref="Basic.ProjectId" />.
    ///   </para>
    ///   <para>
    ///     If the given path is relative, then the absolute <see cref="Basic.ProjectDir" /> will
    ///     be resolved from the <see cref="Basic.BaseDir" />. Note that the <paramref name="projectDir" />
    ///     can be empty.
    ///   </para>
    ///   <para>
    ///     If the given path is absolute, the absolute path will be taken verbatim.
    ///   </para>
    /// </param>
    /// <param name="baseDir">
    ///   <para>
    ///     The directory under which all projects should live. By default this is the directory
    ///     where the <c>Build.cs</c> script is located.
    ///   </para>
    ///   <para>
    ///     By default this is where the <see cref="Basic.BuildDir" /> will be located.
    ///   </para>
    /// </param>
    public static Conf CsApp(string projectId,
                             Option<string> projectDir = default(Option<string>),
                             Option<string> baseDir = default(Option<string>))
      => CsLib(projectId, projectDir, baseDir)
        .Modify(CsCompilationOptions, (_, oldValue) => oldValue.WithOutputKind(OutputKind.ConsoleApplication));

    public static Conf EmbedResource(this Conf conf, string path, string nameInAssembly)
      => conf.Add(EmbeddedResources, c => {
        var resourceFile = Path.IsPathRooted(path) ? path : Path.Combine(ProjectDir[c], path);
        return ToResourceDescriptor(resourceFile, nameInAssembly);
      });

    private static IObservable<IImmutableList<PackageFile>> AssembliesPackagedPaths(IConf c)
      => AssemblyReferences[c].Select<IImmutableList<string>, IEnumerable<PackageFile>>(PackageAssemblies)
                              .Select(ImmutableList.ToImmutableList);

    private static IEnumerable<PackageFile> PackageAssemblies(IImmutableList<string> files)
      => from assemblyPath in files
         let assemblyFileName = Path.GetFileName(assemblyPath)
         where !WindowsFrameworkAssemblyResolver.IsFrameworkAssembly(assemblyFileName)
         select new PackageFile(assemblyPath, assemblyFileName);

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
        Console.WriteLine($"Compiled '{Path.GetFileNameWithoutExtension(output.AssemblyPath)}' in {output.CompilationTime.Milliseconds}ms.");
      } else {
        Console.WriteLine($"Failed to compile '{output.AssemblyPath}'.");
        foreach (var diagnostic in output.Diagnostics) {
          Console.WriteLine(diagnostic);
        }
      }
    }

    private static ResourceDescription ToResourceDescriptor(string resourceFile, string nameInAssembly)
      => new ResourceDescription(nameInAssembly, () => File.OpenRead(resourceFile), true);

    private static IObservable<IEnumerable<CompileOutput>> ObserveDependencies(IConf c)
      => Dependencies[c].Gather(dependency => c.TryGet(dependency/Compile))
                              .Combined();

    private static IEnumerable<PackageFile> ToPackageFiles(IEnumerable<string> dlls)
      => dlls.Select(dll => new PackageFile(dll, $"lib/{Path.GetFileName(dll)}"));

    private static IObservable<IEnumerable<PackageFile>> PackageLibDlls(IConf c)
      => Output[c].Select(ToPackageFiles);

    private static IEnumerable<string> DefaultExcludedSourceDirs(IConf c)
      => new[] {
        "obj",
        "bin",
        (PackagesSubProjectId/ProjectDir)[c]
      };
  }
}