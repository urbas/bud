using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bud.Configuration;
using Bud.Cs;
using Bud.IO;
using Bud.NuGet;
using Bud.Optional;
using Bud.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NuGet.Packaging;
using static System.IO.Path;

namespace Bud.V1 {
  /// <summary>
  ///   Defines the core concepts of every build in Bud.
  ///   <para>
  ///     Every build has an ID and a directory.
  ///   </para>
  ///   <para>
  ///     In addition, every build has three observable streams: input, build, and output.
  ///     The input is piped (unmodified) through to the build and then frurther
  ///     through to output.
  ///   </para>
  ///   <para>
  ///     The build is defined entirely through keys defined in this class. For example,
  ///     the input, build, and output are defined with keys <see cref="Api.Input" />,
  ///     <see cref="Api.Build" />, and <see cref="Conf.Out" />. One can customise these through
  ///     the <see cref="Conf" /> API (such as the <see cref="Conf.Modify{T}" /> method).
  ///   </para>
  /// </summary>
  public static class Api {
    #region Project Grouping

    public static Conf Project(string scope, params IConfBuilder[] confs)
      => Conf.Group(scope, confs);

    public static Conf Projects(params IConfBuilder[] confs)
      => Conf.Group((IEnumerable<IConfBuilder>) confs);

    public static Conf Projects(IEnumerable<IConfBuilder> confs)
      => Conf.Group(confs);

    #endregion

    #region Build Support

    /// <summary>
    ///   Input is an observable stream of collections of files. Whenever input
    ///   files change, a new observation is made in this input stream.
    /// </summary>
    public static readonly Key<IObservable<IEnumerable<string>>> Input = nameof(Input);

    /// <summary>
    ///   By default, build produces a single empty output.
    /// </summary>
    public static readonly Key<IObservable<IEnumerable<string>>> Build = nameof(Build);

    /// <summary>
    ///   By default, output forwards the result of the build without modifications.
    /// </summary>
    public static readonly Key<IObservable<IEnumerable<string>>> Output = nameof(Output);

    public static Conf BuildSupport = Conf
      .Empty
      .InitEmpty(Input)
      .InitEmpty(Build)
      .Init(Output, c => Build[c]);

    #endregion

    #region Dependencies Support

    /// <summary>
    ///   A list of keys (paths) to other builds. For example, say we defined two projects
    ///   <c>A</c> and <c>B</c>. To make <c>B</c> depend on <c>A</c>, one would add the
    ///   <c>../A</c> to the <see cref="Dependencies" /> list.
    /// </summary>
    public static readonly Key<IImmutableSet<string>> Dependencies = nameof(Dependencies);

    /// <summary>
    ///   This observable stream contains output from all dependencies.
    /// </summary>
    public static readonly Key<IObservable<IEnumerable<string>>> DependenciesInput = nameof(DependenciesInput);

    public static readonly Conf DependenciesSupport = Conf
      .Empty
      .InitEmpty(Dependencies)
      .Init(DependenciesInput, GatherOutputsFromDependencies);

    private static IObservable<IEnumerable<string>> GatherOutputsFromDependencies(IConf c)
      => Dependencies[c]
        .Gather(dependency => c.TryGet(dependency/Output))
        .Combined();

    #endregion

    #region Build Pipeline Scheduling Support

    /// <summary>
    ///   By default the entire build pipeline (input, sources, build, and output) are
    ///   scheduled on the same scheduler and the same thread (i.e.: the build pipeline
    ///   is single threaded). The build pipeline is also asynchronous. For example,
    ///   compilers can run each in their own thread and produce output when they finish.
    ///   The output is collected in the build pipeline's thread.
    /// </summary>
    /// <remarks>
    ///   You should never need to override this outside of testing. In all honesty, this
    ///   key is mostly meant for testing.
    /// </remarks>
    public static readonly Key<IScheduler> BuildPipelineScheduler = nameof(BuildPipelineScheduler);

    private static readonly Lazy<EventLoopScheduler> DefauBuildPipelineScheduler =
      new Lazy<EventLoopScheduler>(() => new EventLoopScheduler());

    public static readonly Conf BuildSchedulingSupport =
      BuildPipelineScheduler.Init(_ => DefauBuildPipelineScheduler.Value);

    #endregion

    #region Sources Support

    /// <summary>
    ///   By default, the build has no sources. Add them through
    ///   <see cref="AddSources" /> or <see cref="AddSourceFiles" />.
    /// </summary>
    public static readonly Key<IObservable<IImmutableList<string>>> Sources = nameof(Sources);

    /// <summary>
    ///   A descriptor of where to fetch source files from and how to
    ///   watch for their changes.
    ///   <para>
    ///     By default, these sources are passed through <see cref="SourceExcludeFilters" />
    ///     and then passed on to <see cref="Sources" />.
    ///   </para>
    /// </summary>
    public static readonly Key<IImmutableList<FileWatcher>> SourceIncludes = nameof(SourceIncludes);

    /// <summary>
    ///   These filters are applied on the <see cref="Sources" /> stream
    ///   before it is passed to <see cref="SourceProcessors" />.
    /// </summary>
    public static readonly Key<IImmutableList<Func<string, bool>>> SourceExcludeFilters = nameof(SourceExcludeFilters);

    /// <summary>
    ///   How long to wait after a file change has been noticed before triggering
    ///   a build (i.e.: producing an observation). For example, <see cref="ProcessedSources" />
    ///   are guarded with this calming period.
    /// </summary>
    public static readonly Key<TimeSpan> WatchedFilesCalmingPeriod = nameof(WatchedFilesCalmingPeriod);

    /// <summary>
    ///   This observatory is used when watching source file changes (see <see cref="Sources" />).
    /// </summary>
    /// <remarks>
    ///   You should never need to override this outside of testing. In all honesty, this
    ///   key is mostly meant for testing.
    /// </remarks>
    public static readonly Key<IFilesObservatory> FilesObservatory = nameof(FilesObservatory);

    public static readonly Conf SourcesSupport = BuildSchedulingSupport
      .InitEmpty(SourceIncludes)
      .InitEmpty(SourceExcludeFilters)
      .InitValue(WatchedFilesCalmingPeriod, TimeSpan.FromMilliseconds(300))
      .InitValue(FilesObservatory, new LocalFilesObservatory())
      .Init(Sources, DefaultSources);

    /// <summary>
    ///   Adds an individual source file to the project.
    /// </summary>
    public static Conf AddSourceFile(this Conf c, string absolutePath)
      => c.Add(SourceIncludes,
               conf => FilesObservatory[conf].WatchFiles(absolutePath));

    /// <summary>
    ///   Adds an individual source file to the project.
    /// </summary>
    public static Conf AddSourceFile(this Conf c, Func<IConf, string> absolutePath)
      => c.Add(SourceIncludes,
               conf => FilesObservatory[conf].WatchFiles(absolutePath(conf)));

    private static IObservable<IImmutableList<string>> DefaultSources(IConf c)
      => SourceIncludes[c].ToObservable(SourceFilter(c)).ObserveOn(BuildPipelineScheduler[c])
                          .Calmed(c)
                          .Select(ImmutableList.ToImmutableList);

    private static Func<string, bool> SourceFilter(IConf c) {
      var excludeFilters = SourceExcludeFilters[c];
      return sourceFile => !excludeFilters.Any(filter => filter(sourceFile));
    }

    #endregion

    #region Source Processing Support

    /// <summary>
    ///   A stream of <see cref="Sources" /> after they have been processed
    ///   by <see cref="SourceProcessors" />.
    /// </summary>
    public static readonly Key<IObservable<IImmutableList<string>>> ProcessedSources = nameof(ProcessedSources);

    /// <summary>
    ///   <see cref="Sources" /> are passed through source processors in order.
    ///   Their output is then piped through <c>ProcessedSources</c>.
    /// </summary>
    public static readonly Key<IImmutableList<IInputProcessor>> SourceProcessors = nameof(SourceProcessors);

    public static readonly Conf SourceProcessorsSupport = SourcesSupport
      .InitEmpty(SourceProcessors)
      .Init(ProcessedSources, DefaultProcessSources);

    private static IObservable<IImmutableList<string>> DefaultProcessSources(IConf project)
      => SourceProcessors[project]
        .Aggregate(Sources[project] as IObservable<IEnumerable<string>>,
                   (sources, processor) => processor.Process(sources))
        .Select(ImmutableList.ToImmutableList);

    #endregion

    #region Bare Project

    public const string TargetDirName = "target";

    /// <summary>
    ///   The build's identifier. This identifier is used in <see cref="Dependencies" />.
    /// </summary>
    public static readonly Key<string> ProjectId = nameof(ProjectId);

    /// <summary>
    ///   The build's directory. Ideally, all the sources of this build
    ///   should be located within this directory.
    /// </summary>
    public static readonly Key<string> ProjectDir = nameof(ProjectDir);

    /// <summary>
    ///   The directory where all outputs and generated files are placed.
    ///   This directory is by default deleted through the <see cref="Clean" />
    ///   command.
    /// </summary>
    public static readonly Key<string> TargetDir = nameof(TargetDir);

    /// <summary>
    ///   By default, deletes the entire <see cref="TargetDir" />
    /// </summary>
    public static readonly Key<Unit> Clean = nameof(Clean);

    /// <param name="projectDir">see <see cref="ProjectDir" /></param>
    /// <param name="projectId">see <see cref="ProjectId" /></param>
    public static Conf BareProject(string projectDir, string projectId)
      => Project(projectId)
        .Add(BuildSchedulingSupport)
        .InitValue(ProjectDir, projectDir)
        .Init(TargetDir, c => Combine(ProjectDir[c], TargetDirName))
        .InitValue(ProjectId, projectId)
        .Init(Clean, DefaultClean);

    private static Unit DefaultClean(IConf c) {
      var targetDir = TargetDir[c];
      if (Directory.Exists(targetDir)) {
        Directory.Delete(targetDir, true);
      }
      return Unit.Default;
    }

    #endregion

    #region Build Project

    /// <param name="projectDir">see <see cref="ProjectDir" /></param>
    /// <param name="projectId">see <see cref="ProjectId" /></param>
    public static Conf BuildProject(string projectDir, string projectId)
      => BareProject(projectDir, projectId)
        .Add(BuildSupport)
        .Add(DependenciesSupport)
        .Add(SourceProcessorsSupport)
        .Add(Input, c => ProcessedSources[c])
        .ExcludeSourceDir(c => TargetDir[c]);

    /// <summary>
    ///   Adds files found in <paramref name="subDir" /> to <see cref="Sources" />.
    /// </summary>
    /// <param name="c">the project to which to add sources.</param>
    /// <param name="subDir">a directory relative to <see cref="ProjectDir" />.</param>
    /// <param name="fileFilter">
    ///   a wildcard-based filter of files to collect
    ///   from <paramref name="subDir" />.
    /// </param>
    /// <param name="includeSubdirs">
    ///   indicates whether files in sub-directories
    ///   of <paramref name="subDir" /> should be included.
    /// </param>
    /// <returns>the modified project</returns>
    public static Conf AddSources(this Conf c, string subDir = null, string fileFilter = "*", bool includeSubdirs = true)
      => c.Add(SourceIncludes, conf => {
        var sourceDir = subDir == null ? ProjectDir[conf] : Combine(ProjectDir[conf], subDir);
        return FilesObservatory[conf].WatchDir(sourceDir, fileFilter, includeSubdirs);
      });

    /// <summary>
    ///   Adds individual source files to the project.
    /// </summary>
    public static Conf AddSourceFiles(this Conf c, params string[] relativeFilePaths)
      => c.Add(SourceIncludes, conf => {
        var projectDir = ProjectDir[conf];
        var absolutePaths = relativeFilePaths.Select(relativeFilePath => Combine(projectDir, relativeFilePath));
        return FilesObservatory[conf].WatchFiles(absolutePaths);
      });

    /// <summary>
    ///   Removes the given list of subdirectories from sources.
    /// </summary>
    public static Conf ExcludeSourceDirs(this Conf c, params string[] subDirs)
      => c.ExcludeSourceDirs(_ => subDirs);

    /// <summary>
    ///   Removes the given subdirectory from sources.
    /// </summary>
    public static Conf ExcludeSourceDir(this Conf c, Func<IConf, string> subDir)
      => c.ExcludeSourceDirs(conf => new[] {subDir(conf)});

    /// <summary>
    ///   Removes the given list of subdirectories from sources.
    /// </summary>
    public static Conf ExcludeSourceDirs(this Conf c, Func<IConf, IEnumerable<string>> subDirs)
      => c.Add(SourceExcludeFilters, conf => {
        var projectDir = ProjectDir[conf];
        var dirs = subDirs(conf).Select(s => IsPathRooted(s) ? s : Combine(projectDir, s));
        return PathUtils.InAnyDirFilter(dirs);
      });

    private static IObservable<T> Calmed<T>(this IObservable<T> observable, IConf c)
      => observable.CalmAfterFirst(WatchedFilesCalmingPeriod[c], BuildPipelineScheduler[c]);

    #endregion

    #region CSharp Projects

    public static readonly Key<IObservable<CompileOutput>> Compile = nameof(Compile);
    public static readonly Key<Func<CompileInput, CompileOutput>> Compiler = nameof(Compiler);
    public static readonly Key<IImmutableList<string>> AssemblyReferences = nameof(AssemblyReferences);
    public static readonly Key<string> AssemblyName = nameof(AssemblyName);
    public static readonly Key<CSharpCompilationOptions> CSharpCompilationOptions = nameof(CSharpCompilationOptions);
    public static readonly Key<IImmutableList<ResourceDescription>> EmbeddedResources = nameof(EmbeddedResources);

    public static Conf CsLibrary(string projectId)
      => CsLibrary(projectId, projectId);

    public static Conf CsLibrary(string projectDir, string projectId)
      => BuildProject(projectDir, projectId)
        .AddSources(fileFilter: "*.cs")
        .ExcludeSourceDirs("obj", "bin", TargetDirName)
        .Init(Compile, DefaultCSharpCompilation)
        .Add(Build, c => Compile[c].Select(output => output.AssemblyPath))
        .Init(AssemblyName, c => ProjectId[c] + CSharpCompilationOptions[c].OutputKind.ToExtension())
        .InitEmpty(AssemblyReferences)
        .InitEmpty(EmbeddedResources)
        .Init(Compiler, TimedEmittingCompiler.Create)
        .InitValue(CSharpCompilationOptions, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, warningLevel: 1));

    public static Conf EmbedResource(this Conf conf, string path, string nameInAssembly)
      => conf.Add(EmbeddedResources, c => {
        var resourceFile = IsPathRooted(path) ? path : Combine(ProjectDir[c], path);
        return ToResourceDescriptor(resourceFile, nameInAssembly);
      });

    private static IObservable<CompileOutput> DefaultCSharpCompilation(IConf conf)
      => Input[conf].CombineLatest(ObserveDependencies(conf),
                                   (sources, dependencies) => new CompileInput(
                                     sources,
                                     dependencies,
                                     AssemblyReferences[conf]
                                     )).Select(Compiler[conf]).Do(PrintCompilationResult);

    private static void PrintCompilationResult(CompileOutput output) {
      if (output.Success) {
        Console.WriteLine($"Compiled '{GetFileNameWithoutExtension(output.AssemblyPath)}' in {output.CompilationTime.Milliseconds}ms.");
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

    #endregion

    #region Package Reference Projects

    /// <summary>
    ///   The path to the <c>packages.config</c> file. By default, it is placed directly
    ///   under the <see cref="ProjectDir" />.
    /// </summary>
    public static Key<string> PackagesConfigFile = nameof(PackagesConfigFile);

    /// <summary>
    ///   A list of paths to assemblies. These paths are resolved from NuGet
    ///   package references.
    /// </summary>
    public static Key<IObservable<IImmutableList<string>>> Assemblies = nameof(Assemblies);

    public static Key<IAssemblyResolver> AssemblyResolver = nameof(AssemblyResolver);

    public static Conf PackageReferencesProject(string dir, string projectId)
      => BareProject(dir, projectId)
        .Add(SourcesSupport)
        .AddSourceFile(c => PackagesConfigFile[c])
        .InitValue(AssemblyResolver, new NuGetAssemblyResolver())
        .Init(PackagesConfigFile, c => Combine(ProjectDir[c], "packages.config"))
        .Init(Assemblies, ResolveAssemblies);

    private static IObservable<IImmutableList<string>> ResolveAssemblies(IConf c)
      => Sources[c].Select(sources => {
        var resolvedAssembliesFile = Combine(TargetDir[c], "resolved_assemblies");
        if (File.Exists(resolvedAssembliesFile) && FileUtils.IsNewerThan(resolvedAssembliesFile, sources)) {
          return File.ReadAllLines(resolvedAssembliesFile)
                     .ToImmutableList();
        }
        var packageReferences = sources.Where(File.Exists)
                                       .SelectMany(PackageReferencesFromFile);
        var assemblies = AssemblyResolver[c].ResolveAssemblies(packageReferences)
                                            .ToImmutableList();
        Directory.CreateDirectory(TargetDir[c]);
        File.WriteAllLines(resolvedAssembliesFile, assemblies);
        return assemblies;
      });

    private static IEnumerable<PackageReference> PackageReferencesFromFile(string source)
      => new PackagesConfigReader(File.OpenRead(source))
        .GetPackages();

    #endregion
  }
}