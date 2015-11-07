using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using Bud.IO;
using static System.IO.Path;
using static Bud.Conf;

namespace Bud {
  public static class Build {
    public static readonly Key<string> ProjectDir = nameof(ProjectDir);
    public static readonly Key<string> ProjectId = nameof(ProjectId);
    public static readonly Key<Files> Sources = nameof(Sources);
    public static readonly Key<IEnumerable<string>> Dependencies = nameof(Dependencies);
    public static readonly Key<IFilesObservatory> FilesObservatory = nameof(FilesObservatory);
    public static readonly Key<IScheduler> BuildPipelineScheduler = nameof(BuildPipelineScheduler);

    public static Conf Project(string projectDir, string projectId = null)
      => Empty.InitValue(ProjectDir, projectDir)
              .Init(ProjectId, c => projectId ?? GetFileName(ProjectDir[c]))
              .InitValue(Sources, Files.Empty)
              .Init(BuildPipelineScheduler, conf => new EventLoopScheduler())
              .InitValue(Dependencies, Enumerable.Empty<string>())
              .Init(FilesObservatory, c => new LocalFilesObservatory());

    public static Conf SourceDir(string subDir = null, string fileFilter = "*", bool includeSubdirs = true) {
      return Empty.Modify(Sources, (configs, sources) => {
        var sourceDir = subDir == null ? ProjectDir[configs] : Combine(ProjectDir[configs], subDir);
        var newSources = FilesObservatory[configs].ObserveDir(sourceDir, fileFilter, includeSubdirs);
        return sources.ExpandWith(newSources);
      });
    }

    public static Conf SourceFiles(params string[] relativeFilePaths)
      => Empty.Modify(Sources, (configs, existingSources) => {
        var projectDir = ProjectDir[configs];
        var absolutePaths = relativeFilePaths.Select(relativeFilePath => Combine(projectDir, relativeFilePath));
        var newSources = FilesObservatory[configs].ObserveFiles(absolutePaths);
        return existingSources.ExpandWith(newSources);
      });

    public static Conf ExcludeSourceDirs(params string[] subDirs)
      => Empty.Modify(Sources, (configs, previousFiles) => {
        var forbiddenDirs = subDirs.Select(s => Combine(ProjectDir[configs], s));
        return previousFiles.WithFilter(file => !forbiddenDirs.Any(file.StartsWith));
      });
  }
}