using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bud.IO;
using Bud.Reactive;
using static System.IO.Path;
using static Bud.Conf;
using static Bud.IO.PathUtils;
using static Bud.Reactive.ObservableResources;

namespace Bud {
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
  ///     the input, build, and output are defined with keys <see cref="Input" />,
  ///     <see cref="Build" />, and <see cref="Conf.Out" />. One can customise these through
  ///     the <see cref="Conf" /> API (such as the <see cref="Conf.Modify{T}" /> method).
  ///   </para>
  /// </summary>
  public static class Builds {
    /// <summary>
    ///   By default, input consists of <see cref="ProcessedSources" /> and
    ///   outputs of <see cref="Dependencies" />.
    /// </summary>
    public static readonly Key<IObservable<InOut>> Input = nameof(Input);

    /// <summary>
    ///   By default, build simply pipes the input through.
    /// </summary>
    public static readonly Key<IObservable<InOut>> Build = nameof(Build);

    /// <summary>
    ///   By default, output simply pipes the build through.
    /// </summary>
    public static readonly Key<IObservable<InOut>> Output = nameof(Output);

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
    ///   A list of keys (paths) to other builds. For example, say we defined two projects
    ///   <c>A</c> and <c>B</c>. To make <c>B</c> depend on <c>A</c>, one would add the
    ///   <c>../A</c> to the <see cref="Dependencies" /> list.
    /// </summary>
    public static readonly Key<IImmutableSet<string>> Dependencies = nameof(Dependencies);

    /// <summary>
    ///   By default, deletes the entire <see cref="TargetDir" />
    /// </summary>
    public static readonly Key<Unit> Clean = nameof(Clean);

    /// <summary>
    ///   By default, the build has no sources. Add them through
    ///   <see cref="AddSources" /> or <see cref="AddSourceFiles" />.
    /// </summary>
    public static readonly Key<IObservable<IEnumerable<string>>> Sources = nameof(Sources);

    /// <summary>
    ///   A descriptor of where to fetch source files from and how to
    ///   watch for their changes.
    ///   <para>
    ///     By default, these sources are passed through <see cref="SourceExcludeFilters" />
    ///     and then passed on to <see cref="Sources" />.
    ///   </para>
    /// </summary>
    public static readonly Key<IImmutableList<Watched<string>>> SourceIncludes = nameof(SourceIncludes);

    /// <summary>
    ///   These filters are applied on the <see cref="Sources" /> stream
    ///   before it is passed to <see cref="SourceProcessors" />.
    /// </summary>
    public static readonly Key<IImmutableList<Func<string, bool>>> SourceExcludeFilters = nameof(SourceExcludeFilters);

    /// <summary>
    ///   A stream of <see cref="Sources" /> after they have been processed
    ///   by <see cref="SourceProcessors" />.
    /// </summary>
    public static readonly Key<IObservable<InOut>> ProcessedSources = nameof(ProcessedSources);

    /// <summary>
    ///   <see cref="Sources" /> are passed through source processors in order.
    ///   Their output is then piped through <c>ProcessedSources</c>.
    /// </summary>
    public static readonly Key<IImmutableList<IInputProcessor>> SourceProcessors = nameof(SourceProcessors);

    /// <summary>
    ///   How long to wait after a file change has been noticed before triggering
    ///   a build (i.e.: producing an observation). For example, <see cref="ProcessedSources" />
    ///   are guarded with this calming period.
    /// </summary>
    public static readonly Key<TimeSpan> WatchedFilesCalmingPeriod = nameof(WatchedFilesCalmingPeriod);

    /// <summary>
    ///   By default the entire build pipeline (input, sources, build, and output) are
    ///   scheduled on the same scheduler and the same thread (i.e.: the build pipeline
    ///   is single threaded). The build pipeline is, however, asynchronous. For example,
    ///   compilers can run each in their own
    ///   thread and produce output whenever they finish. The output is
    ///   collected in the build pipeline's thread.
    /// </summary>
    /// <remarks>
    ///   You should never need to override this outside of testing. In all honesty, this
    ///   key is mostly meant for testing.
    /// </remarks>
    public static readonly Key<IScheduler> BuildPipelineScheduler = nameof(BuildPipelineScheduler);

    /// <summary>
    ///   This observatory is used when watching source file changes (see <see cref="Sources" />).
    /// </summary>
    /// <remarks>
    ///   You should never need to override this outside of testing. In all honesty, this
    ///   key is mostly meant for testing.
    /// </remarks>
    public static readonly Key<IFilesObservatory> FilesObservatory = nameof(FilesObservatory);

    private static readonly Lazy<EventLoopScheduler> DefauBuildPipelineScheduler = new Lazy<EventLoopScheduler>(() => new EventLoopScheduler());

    /// <param name="projectDir">see <see cref="ProjectDir" /></param>
    /// <param name="projectId">see <see cref="ProjectId" /></param>
    /// <returns>
    ///   a configuration containing the definition of a
    ///   simple Bud build project.
    /// </returns>
    public static Conf Project(string projectDir, string projectId)
      => Group(projectId)
        .InitValue(ProjectDir, projectDir)
        .InitValue(ProjectId, projectId)
        .Init(TargetDir, c => Combine(ProjectDir[c], "target"))
        .InitValue(SourceIncludes, ImmutableList<Watched<string>>.Empty)
        .InitValue(SourceExcludeFilters, ImmutableList<Func<string, bool>>.Empty)
        .Init(Sources, DefaultSources)
        .InitValue(SourceProcessors, ImmutableList<IInputProcessor>.Empty)
        .Init(ProcessedSources, DefaultProcessSources)
        .Init(Input, DefaultInput)
        .Init(Build, c => Input[c])
        .Init(Output, c => Build[c])
        .Init(Clean, DefaultClean)
        .InitValue(Dependencies, ImmutableHashSet<string>.Empty)
        .Init(BuildPipelineScheduler, _ => DefauBuildPipelineScheduler.Value)
        .InitValue(WatchedFilesCalmingPeriod, TimeSpan.FromMilliseconds(300))
        .Init(FilesObservatory, _ => new LocalFilesObservatory())
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
      => c.Modify(SourceIncludes, (conf, sources) => {
        var sourceDir = subDir == null ? ProjectDir[conf] : Combine(ProjectDir[conf], subDir);
        var newSources = FilesObservatory[conf].ObserveDir(sourceDir, fileFilter, includeSubdirs);
        return sources.Add(newSources);
      });

    /// <summary>
    ///   Adds individual source files to the project.
    /// </summary>
    public static Conf AddSourceFiles(this Conf c, params string[] relativeFilePaths)
      => c.Modify(SourceIncludes, (conf, existingSources) => {
        var projectDir = ProjectDir[conf];
        var absolutePaths = relativeFilePaths.Select(relativeFilePath => Combine(projectDir, relativeFilePath));
        var newSources = FilesObservatory[conf].ObserveFiles(absolutePaths);
        return existingSources.Add(newSources);
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
      => c.ExcludeSourceDirs(conf => new [] {subDir(conf)});

    /// <summary>
    ///   Removes the given list of subdirectories from sources.
    /// </summary>
    public static Conf ExcludeSourceDirs(this Conf c, Func<IConf, IEnumerable<string>> subDirs)
      => c.Modify(SourceExcludeFilters, (conf, filters) => {
        var projectDir = ProjectDir[conf];
        var dirs = subDirs(conf).Select(s => IsPathRooted(s) ? s : Combine(projectDir, s));
        return filters.Add(NotInAnyDirFilter(dirs));
      });

    public static IObservable<T> Calmed<T>(this IObservable<T> observable, IConf c)
      => observable.CalmAfterFirst(WatchedFilesCalmingPeriod[c],
                                   BuildPipelineScheduler[c]);

    private static IObservable<InOut> DefaultProcessSources(IConf project)
      => SourceProcessors[project]
        .Aggregate(ObservedSources(project),
                   (sources, processor) => processor.Process(sources));

    private static IObservable<IEnumerable<string>> DefaultSources(IConf c)
      => ObserveResources(SourceIncludes[c], SourceExcludeFilter(c))
        .ObserveOn(BuildPipelineScheduler[c])
        .Calmed(c);

    private static Func<string, bool> SourceExcludeFilter(IConf c) {
      Func<string, bool> result = _ => true;
      foreach (var filter in SourceExcludeFilters[c]) {
        var oldResult = result;
        result = source => oldResult(source) && filter(source);
      }
      return result;
    }

    private static IObservable<InOut> ObservedSources(IConf c)
      => Sources[c].Select(sources => new InOut(sources.Select(InOutFile.ToInOutFile)));

    private static IObservable<InOut> DefaultInput(IConf c)
      => Dependencies[c].Select(dependency => (dependency / Output)[c])
                        .Concat(new[] {ProcessedSources[c]})
                        .CombineLatest(InOut.Merge);

    private static Unit DefaultClean(IConf c) {
      var targetDir = Combine(ProjectDir[c], "target");
      if (Directory.Exists(targetDir)) {
        Directory.Delete(targetDir, true);
      }
      return Unit.Default;
    }
  }
}