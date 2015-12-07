using System;
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

namespace Bud {
  public static class Builds {
    public static readonly Key<IObservable<InOut>> Input = nameof(Input);
    public static readonly Key<IObservable<InOut>> Build = nameof(Build);
    public static readonly Key<IObservable<InOut>> Output = nameof(Output);
    public static readonly Key<Unit> Clean = nameof(Clean);
    public static readonly Key<Files> Sources = nameof(Sources);
    public static readonly Key<string> ProjectId = nameof(ProjectId);
    public static readonly Key<string> ProjectDir = nameof(ProjectDir);
    public static readonly Key<IImmutableSet<string>> Dependencies = nameof(Dependencies);
    public static readonly Key<IFilesObservatory> FilesObservatory = nameof(FilesObservatory);
    public static readonly Key<IScheduler> BuildPipelineScheduler = nameof(BuildPipelineScheduler);
    public static readonly Key<IObservable<InOut>> ProcessedSources = nameof(ProcessedSources);
    public static readonly Key<IImmutableList<IFilesProcessor>> SourceProcessors = nameof(SourceProcessors);
    public static readonly Key<TimeSpan> InputCalmingPeriod = nameof(InputCalmingPeriod);
    private static readonly Lazy<EventLoopScheduler> DefauBuildPipelineScheduler = new Lazy<EventLoopScheduler>(() => new EventLoopScheduler());

    public static Conf Project(string projectDir, string projectId)
      => Group(projectId)
        .InitValue(ProjectDir, projectDir)
        .InitValue(ProjectId, projectId)
        .InitValue(Sources, Files.Empty)
        .Init(Input, DefaultInput)
        .Init(Build, c => Input[c])
        .Init(Output, c => Build[c])
        .InitValue(Dependencies, ImmutableHashSet<string>.Empty)
        .Init(BuildPipelineScheduler, _ => DefauBuildPipelineScheduler.Value)
        .Init(ProcessedSources, ProcessSources)
        .InitValue(InputCalmingPeriod, TimeSpan.FromMilliseconds(300))
        .InitValue(SourceProcessors, ImmutableList<IFilesProcessor>.Empty)
        .Init(FilesObservatory, _ => new LocalFilesObservatory())
        .Init(Clean, DefaultClean);

    public static Conf SourceDir(string subDir = null, string fileFilter = "*", bool includeSubdirs = true) {
      return Empty.Modify(Sources, (conf, sources) => {
        var sourceDir = subDir == null ? ProjectDir[conf] : Combine(ProjectDir[conf], subDir);
        var newSources = FilesObservatory[conf].ObserveDir(sourceDir, fileFilter, includeSubdirs);
        return sources.ExpandWith(newSources);
      });
    }

    public static Conf SourceFiles(params string[] relativeFilePaths)
      => Empty.Modify(Sources, (conf, existingSources) => {
        var projectDir = ProjectDir[conf];
        var absolutePaths = relativeFilePaths.Select(relativeFilePath => Combine(projectDir, relativeFilePath));
        var newSources = FilesObservatory[conf].ObserveFiles(absolutePaths);
        return existingSources.ExpandWith(newSources);
      });

    public static Conf ExcludeSourceDirs(params string[] subDirs)
      => Empty.Modify(Sources, (conf, previousFiles) => {
        var baseDir = ProjectDir[conf];
        var dirs = subDirs.Select(s => Combine(baseDir, s));
        return previousFiles.WithFilter(NotInAnyDirFilter(dirs));
      });

    public static IObservable<T> Calmed<T>(this IObservable<T> observable, IConf conf)
      => observable.CalmAfterFirst(InputCalmingPeriod[conf], BuildPipelineScheduler[conf]);

    private static IObservable<InOut> ProcessSources(IConf project)
      => SourceProcessors[project]
        .Aggregate(ObservedSources(project),
                   (sources, processor) => processor.Process(sources));

    private static IObservable<InOut> ObservedSources(IConf c)
      => Sources[c].Watch()
                   .ObserveOn(BuildPipelineScheduler[c])
                   .Calmed(c)
                   .Select(sources => new InOut(sources.Select(InOutFile.ToInOutFile)));

    private static IObservable<InOut> DefaultInput(IConf conf)
      => Dependencies[conf].Select(dependency => (dependency / Output)[conf])
                           .Concat(new[] {ProcessedSources[conf]})
                           .CombineLatest(InOut.Merge);

    private static Unit DefaultClean(IConf conf) {
      var targetDir = Combine(ProjectDir[conf], "target");
      if (Directory.Exists(targetDir)) {
        Directory.Delete(targetDir, true);
      }
      return Unit.Default;
    }
  }
}