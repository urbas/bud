using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using Bud.Pipeline;
using static System.IO.Path;
using static Bud.Conf;

namespace Bud {
  public static class Build {
    public static readonly Key<string> ProjectDir = nameof(ProjectDir);
    public static readonly Key<string> ProjectId = nameof(ProjectId);
    public static readonly Key<IObservable<IEnumerable<string>>> Sources = nameof(Sources);
    public static readonly Key<IFilesObservatory> FilesObservatory = nameof(FilesObservatory);

    public static Conf Project(string projectDir, string projectId = null)
      => Empty.InitConst(Sources, Observable.Return(Enumerable.Empty<string>()))
              .InitConst(ProjectDir, projectDir)
              .Init(ProjectId, c => projectId ?? GetFileName(ProjectDir[c]))
              .Init(FilesObservatory, c => new LocalFilesObservatory());

    public static Conf SourceDir(string subDir = null, string fileFilter = "*", bool includeSubdirs = true) {
      return Empty.Modify(Sources, (configs, sources) => {
        var sourceDir = subDir == null ? ProjectDir[configs] : Combine(ProjectDir[configs], subDir);
        var newSources = FilesObservatory[configs].ObserveFiles(sourceDir, fileFilter, includeSubdirs);
        return sources.CombineStream(newSources);
      });
    }

    public static Conf SourceFiles(params string[] relativeFilePaths)
      => Empty.Modify(Sources, (configs, existingSources) => {
        var projectDir = ProjectDir[configs];
        var absolutePaths = relativeFilePaths.Select(relativeFilePath => Combine(projectDir, relativeFilePath));
        var newSources = FilesObservatory[configs].ObserveFileList(absolutePaths);
        return existingSources.CombineStream(newSources);
      });

    public static Conf ExcludeSourceDirs(params string[] subDirs)
      => Empty.Modify(Sources, (configs, previousFiles) => {
        var forbiddenDirs = subDirs.Select(s => Combine(ProjectDir[configs], s));
        return previousFiles.Select(enumerable => enumerable.Where(file => !forbiddenDirs.Any(file.StartsWith)));
      });
  }
}