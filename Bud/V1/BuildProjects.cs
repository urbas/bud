using System;
using System.Collections.Generic;
using System.Linq;
using Bud.IO;
using Bud.Reactive;
using static System.IO.Path;
using static Bud.V1.Api;

namespace Bud.V1 {
  internal static class BuildProjects {
    internal static readonly Conf BuildProjectSettings = Conf
      .Empty
      .Add(BuildSupport)
      .Add(DependenciesSupport)
      .Add(SourceProcessorsSupport)
      .Add(Input, c => ProcessedSources[c])
      .ExcludeSourceDir(c => TargetDir[c]);

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
        return PathUtils.InAnyDirFilter(dirs);
      });

    internal static IObservable<T> Calmed<T>(this IObservable<T> observable, IConf c)
      => observable.CalmAfterFirst(WatchedFilesCalmingPeriod[c], BuildPipelineScheduler[c]);
  }
}