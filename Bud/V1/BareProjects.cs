using System.Reactive;
using static System.IO.Directory;
using static System.IO.Path;
using static Bud.V1.Api;

namespace Bud.V1 {
  internal static class BareProjects {
    internal static Conf CreateBareProject(string projectDir, string projectId, string version = DefaultVersion)
      => Project(projectId)
        .Add(ApiImpl.BuildSchedulingSupport)
        .InitValue(ProjectId, projectId)
        .InitValue(ProjectDir, projectDir)
        .InitValue(Version, version)
        .Init(TargetDir, c => Combine(ProjectDir[c], TargetDirName))
        .Init(Clean, DefaultClean);

    internal static Unit DefaultClean(IConf c) {
      var targetDir = TargetDir[c];
      if (Exists(targetDir)) {
        Delete(targetDir, true);
      }
      return Unit.Default;
    }
  }
}