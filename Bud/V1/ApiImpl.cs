using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bud.IO;
using Bud.Reactive;
using Bud.Util;
using static Bud.V1.Api;

namespace Bud.V1 {
  internal static class ApiImpl {
    internal static readonly Lazy<EventLoopScheduler> DefauBuildPipelineScheduler
      = new Lazy<EventLoopScheduler>(() => new EventLoopScheduler());

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

    internal static readonly Conf DependenciesSupport
      = Conf.Empty
            .InitEmpty(Dependencies)
            .Init(DependenciesInput, GatherOutputsFromDependencies);

    internal static IObservable<IEnumerable<string>> GatherOutputsFromDependencies(IConf c)
      => Dependencies[c]
        .Gather(dependency => c.TryGet(dependency/Output))
        .Combined();

    internal static IObservable<IImmutableList<string>> DefaultSources(IConf c)
      => SourceIncludes[c]
        .ToObservable(SourceFilter(c))
        .ObserveOn(BuildPipelineScheduler[c])
        .Calmed(c)
        .Select(ImmutableList.ToImmutableList);

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