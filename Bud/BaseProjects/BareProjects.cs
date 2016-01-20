using System.Reactive;
using Bud.V1;
using static System.IO.Directory;
using static System.IO.Path;
using static Bud.BaseProjects.BuildProjects;
using static Bud.V1.Api;

namespace Bud.BaseProjects {
  internal static class BareProjects {
    internal static readonly Conf DependenciesSupport
      = Conf.Empty.InitEmpty(Dependencies);

    internal static Conf CreateBareProject(string projectDir,
                                           string projectId,
                                           string version = DefaultVersion)
      => Project(projectId)
        .Add(DependenciesSupport)
        .Add(BuildSchedulingSupport)
        .InitValue(ProjectId, projectId)
        .InitValue(ProjectDir, projectDir)
        .InitValue(Version, version)
        .Init(BudDir, c => Combine(ProjectDir[c], BudDirName))
        .Init(Clean, DefaultClean);

    internal static Unit DefaultClean(IConf c) {
      var targetDir = BudDir[c];
      if (Exists(targetDir)) {
        Delete(targetDir, true);
      }
      return Unit.Default;
    }
  }
}