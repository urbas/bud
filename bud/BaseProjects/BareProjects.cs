using System;
using System.Reactive;
using System.Reactive.Concurrency;
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

    internal static Conf BareProject(string projectDir,
                                     string projectId,
                                     string version = DefaultVersion)
      => Project(projectId)
        .Add(DependenciesSupport)
        .Add(BuildSchedulingSupport)
        .Init(ProjectId, projectId)
        .Init(ProjectDir, c => GetProjectDir(c, projectDir))
        .Init(ProjectVersion, version)
        .Init(BuildDir, DefaultBuildDir)
        .Init(Clean, DefaultClean);

    private static string GetProjectDir(IConf c, string projectDir)
      => c.TryGet(BaseDir)
          .OrElse(() => c.TryGet(".."/BaseDir))
          .Map(baseDir => Combine(baseDir, projectDir))
          .GetOrElse(() => Combine(GetCurrentDirectory(), projectDir));

    internal static Unit DefaultClean(IConf c) {
      var targetDir = BuildDir[c];
      if (Exists(targetDir)) {
        Delete(targetDir, true);
      }
      return Unit.Default;
    }

    private static string DefaultBuildDir(IConf c)
      => Combine(BaseDir[c], BuildDirName, "projects", GetDirectoryName(c.Key));
  }
}