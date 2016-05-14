using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using Bud.Reactive;
using Bud.Util;
using static Bud.V1.Basic;

namespace Bud.V1 {
  public static class Builds {
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
    ///   These filters are applied on the <see cref="SourceProcessors" /> stream
    ///   before it is passed to <see cref="Sources" />.
    /// </summary>
    public static readonly Key<IImmutableList<Func<string, bool>>> SourceExcludeFilters = nameof(SourceExcludeFilters);

    /// <summary>
    ///   This observable stream contains aggregated output from all dependencies.
    /// </summary>
    public static readonly Key<IObservable<IEnumerable<string>>> DependenciesOutput = nameof(DependenciesOutput);

    /// <summary>
    ///   How long to wait after a file change has been noticed before triggering
    ///   a build (i.e.: producing an observation). For example, <see cref="ProcessedSources" />
    ///   are guarded with this calming period.
    /// </summary>
    public static readonly Key<TimeSpan> WatchedFilesCalmingPeriod = nameof(WatchedFilesCalmingPeriod);

    /// <summary>
    ///   This observatory is used when watching source file changes (see <see cref="Builds.Sources" />).
    /// </summary>
    /// <remarks>
    ///   You should never need to override this outside of testing. In all honesty, this
    ///   key is mostly meant for testing.
    /// </remarks>
    public static readonly Key<IFilesObservatory> FilesObservatory = nameof(FilesObservatory);

    /// <summary>
    ///   Adds an individual source file to the project.
    /// </summary>
    public static Conf AddSourceFile(this Conf c, Func<IConf, string> absolutePath)
      => c.Add(SourceIncludes,
               conf => FilesObservatory[conf].WatchFiles(absolutePath(conf)));

    /// <summary>
    ///   A stream of <see SourceProcessorsources" /> after they have been processed
    ///   by <see cref="Builds" />.
    /// </summary>
    public static readonly Key<IObservable<IImmutableList<string>>> ProcessedSources = nameof(ProcessedSources);

    /// <summary>
    ///   <see cref="Builds.Sources" /> are passed through source processors in order.
    ///   Their output is then piped through <c>ProcessedSources</c>.
    /// </summary>
    public static readonly Key<IImmutableList<IInputProcessor>> SourceProcessors = nameof(SourceProcessors);

    private static readonly Conf BuildSupport
      = Conf.Empty
            .InitEmpty(Input)
            .InitEmpty(Build).Init(Output, c => Build[c]);

    internal static readonly Conf SourcesSupport
      = BuildSchedulingSupport
        .InitEmpty(SourceIncludes)
        .InitEmpty(SourceExcludeFilters)
        .Init(WatchedFilesCalmingPeriod, TimeSpan.FromMilliseconds(300)).Init(FilesObservatory, new LocalFilesObservatory())
        .Init(Sources, DefaultSources);

    private static readonly Conf SourceProcessorsSupport
      = SourcesSupport
        .InitEmpty(SourceProcessors)
        .Init(ProcessedSources, DefaultProcessSources);

    private static readonly Conf BuildProjectSettings = BuildSupport
      .Add(SourceProcessorsSupport)
      .Add(Input, c => ProcessedSources[c])
      .ExcludeSourceDir(c => BuildDir[c])
      .Init(DependenciesOutput, GatherOutputsFromDependencies);

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
    public static Conf BuildProject(string projectId,
                                    Option<string> projectDir = default(Option<string>),
                                    Option<string> baseDir = default(Option<string>))
      => Project(projectId, projectDir, baseDir)
        .Add(BuildProjectSettings);

    /// <summary>
    ///   Adds an individual source file to the project.
    /// </summary>
    public static Conf AddSourceFile(this Conf c, string absolutePath)
      => c.Add(SourceIncludes,
               conf => FilesObservatory[conf].WatchFiles(absolutePath));

    /// <summary>
    ///   Adds files found in <paramref name="subDir" /> to <see cref="Builds.Sources" />.
    /// </summary>
    /// <param name="c">the project to which to add sources.</param>
    /// <param name="subDir">a directory relative to <see cref="Basic.ProjectDir" />.</param>
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
        var sourceDir = subDir == null ?
                          ProjectDir[conf] :
                          Path.Combine(ProjectDir[conf], subDir);
        return FilesObservatory[conf].WatchDir(sourceDir, fileFilter, includeSubdirs);
      });

    /// <summary>
    ///   Adds individual source files to the project.
    /// </summary>
    public static Conf AddSourceFiles(this Conf c, params string[] relativeFilePaths)
      => c.Add(SourceIncludes, conf => {
        var projectDir = ProjectDir[conf];
        var absolutePaths = relativeFilePaths
          .Select(relativeFilePath => Path.Combine(projectDir,
                                                   relativeFilePath));
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
        var dirs = subDirs(conf)
          .Select(s => Path.IsPathRooted(s) ? s : Path.Combine(projectDir, s));
        return PathUtils.InAnyDirFilter(dirs);
      });

    private static IObservable<T> Calmed<T>(this IObservable<T> observable, IConf c)
      => observable.CalmAfterFirst(WatchedFilesCalmingPeriod[c],
                                   BuildPipelineScheduler[c]);

    private static IObservable<IEnumerable<string>> GatherOutputsFromDependencies(IConf c)
      => Dependencies[c]
        .Gather(dependency => c.TryGet(dependency/Output))
        .Combined();

    private static IObservable<IImmutableList<string>> DefaultSources(IConf c)
      => SourceIncludes[c].ToObservable(SourceFilter(c))
                          .ObserveOn(BuildPipelineScheduler[c])
                          .Calmed(c)
                          .Select(ImmutableList.ToImmutableList);

    private static Func<string, bool> SourceFilter(IConf c) {
      var excludeFilters = SourceExcludeFilters[c];
      return sourceFile => !excludeFilters.Any(filter => filter(sourceFile));
    }

    private static IObservable<IImmutableList<string>> DefaultProcessSources(IConf project)
      => SourceProcessors[project].Aggregate<IInputProcessor, IObservable<IEnumerable<string>>>(
        Sources[project],
        (sources, processor) => processor.Process(sources))
                                  .Select(ImmutableList.ToImmutableList);
  }
}