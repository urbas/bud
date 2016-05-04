using System;
using System.Reactive;
using System.Reactive.Concurrency;
using Bud.Util;
using Bud.V1;
using static System.IO.Directory;
using static System.IO.Path;
using static Bud.V1.Api;

namespace Bud.BaseProjects {
  internal static class BareProjects {
    internal static readonly Conf DependenciesSupport
      = Conf.Empty.InitEmpty(Dependencies);

    private static readonly Lazy<EventLoopScheduler> DefauBuildPipelineScheduler
      = new Lazy<EventLoopScheduler>(() => new EventLoopScheduler());

    internal static readonly Conf BuildSchedulingSupport
      = BuildPipelineScheduler
        .Init(_ => DefauBuildPipelineScheduler.Value);

    internal static Conf BareProject(string projectId,
                                     Option<string> projectDir,
                                     Option<string> baseDir)
      => Project(projectId, baseDir)
        .Add(DependenciesSupport)
        .Add(BuildSchedulingSupport)
        .Init(ProjectId, projectId)
        .Init(ProjectDir, c => GetProjectDir(c, projectDir))
        .Init(ProjectVersion, DefaultVersion)
        .Init(BuildDir, DefaultBuildDir)
        .Init(Clean, DefaultClean);

    private static string GetProjectDir(IConf c, Option<string> projectDir)
      => projectDir.Map(dir => Combine(BaseDir[c], dir))
                   .GetOrElse(() => Combine(BaseDir[c], ProjectId[c]));

    internal static Unit DefaultClean(IConf c) {
      var targetDir = BuildDir[c];
      if (Exists(targetDir)) {
        Delete(targetDir, true);
      }
      return Unit.Default;
    }

    private static string DefaultBuildDir(IConf c)
      => Combine(BaseDir[c], BuildDirName, GetDirectoryName(c.Key));
  }
}