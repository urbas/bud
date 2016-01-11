using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bud.IO;
using Bud.Reactive;
using Bud.Util;
using Bud.V1;
using static System.IO.Path;
using static Bud.IO.PathUtils;
using static Bud.V1.Api;

namespace Bud.BaseProjects {
  internal static class BuildProjects {
    internal static Conf BuildSupport
      = Conf.Empty
            .InitEmpty(Input)
            .InitEmpty(Build)
            .Init(Output, c => Build[c]);

    internal static readonly Conf BuildSchedulingSupport
      = BuildPipelineScheduler
        .Init(_ => DefauBuildPipelineScheduler.Value);

    internal static readonly Conf SourcesSupport
      = BuildSchedulingSupport
        .InitEmpty(SourceIncludes)
        .InitEmpty(SourceExcludeFilters)
        .InitValue(WatchedFilesCalmingPeriod, TimeSpan.FromMilliseconds(300))
        .InitValue(FilesObservatory, new LocalFilesObservatory())
        .Init(Sources, DefaultSources);

    internal static readonly Conf SourceProcessorsSupport
      = SourcesSupport
        .InitEmpty(SourceProcessors)
        .Init(ProcessedSources, DefaultProcessSources);

    internal static readonly Conf BuildProjectSettings = Conf
      .Empty
      .Add(BuildSupport)
      .Add(SourceProcessorsSupport)
      .Add(Input, c => ProcessedSources[c])
      .ExcludeSourceDir(c => TargetDir[c])
      .Init(DependenciesInput, GatherOutputsFromDependencies);

    internal static Conf CreateBuildProject(string projectDir, string projectId)
      => BareProject(projectDir, projectId)
        .Add(BuildProjectSettings);

    internal static Conf AddSourcesImpl(Conf c,
                                        string subDir,
                                        string fileFilter,
                                        bool includeSubdirs)
      => c.Add(SourceIncludes, conf => {
        var sourceDir = subDir == null ?
                          ProjectDir[conf] :
                          Combine(ProjectDir[conf], subDir);
        return FilesObservatory[conf]
          .WatchDir(sourceDir, fileFilter, includeSubdirs);
      });

    internal static Conf AddSourceFilesImpl(Conf c,
                                            string[] relativeFilePaths)
      => c.Add(SourceIncludes, conf => {
        var projectDir = ProjectDir[conf];
        var absolutePaths = relativeFilePaths
          .Select(relativeFilePath => Combine(projectDir,
                                              relativeFilePath));
        return FilesObservatory[conf].WatchFiles(absolutePaths);
      });

    internal static Conf
      ExcludeSourceDirsImpl(Conf c,
                            Func<IConf, IEnumerable<string>> subDirs)
      => c.Add(SourceExcludeFilters, conf => {
        var projectDir = ProjectDir[conf];
        var dirs = subDirs(conf)
          .Select(s => IsPathRooted(s) ? s : Combine(projectDir, s));
        return InAnyDirFilter(dirs);
      });

    internal static IObservable<T> Calmed<T>(this IObservable<T> observable, IConf c)
      => observable.CalmAfterFirst(WatchedFilesCalmingPeriod[c],
                                   BuildPipelineScheduler[c]);

    internal static readonly Lazy<EventLoopScheduler> DefauBuildPipelineScheduler
      = new Lazy<EventLoopScheduler>(() => new EventLoopScheduler());

    internal static IObservable<IEnumerable<string>> GatherOutputsFromDependencies(IConf c)
      => Dependencies[c]
        .Gather(dependency => c.TryGet(dependency/Output))
        .Combined();

    internal static IObservable<IImmutableList<string>> DefaultSources(IConf c)
      => Observable.Select(Observable.ObserveOn(SourceIncludes[c]
                                                  .ToObservable(SourceFilter(c)), BuildPipelineScheduler[c])
                                     .Calmed(c), ImmutableList.ToImmutableList);

    internal static Func<string, bool> SourceFilter(IConf c) {
      var excludeFilters = SourceExcludeFilters[c];
      return sourceFile => !excludeFilters.Any(filter => filter(sourceFile));
    }

    internal static IObservable<IImmutableList<string>> DefaultProcessSources(IConf project)
      => SourceProcessors[project]
        .Aggregate(Sources[project] as IObservable<IEnumerable<string>>,
                   (sources, processor) => processor.Process(sources))
        .Select(ImmutableList.ToImmutableList);
  }
}