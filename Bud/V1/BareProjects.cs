using System.Reactive;
using static System.IO.Directory;
using static System.IO.Path;
using static Bud.V1.Api;

namespace Bud.V1 {
  internal static class BareProjects {
    internal static Conf CreateBareProject(string projectDir, string projectId)
      => Project(projectId)
        .Add(BuildSchedulingSupport)
        .InitValue(ProjectDir, projectDir)
        .Init(TargetDir, c => Combine(ProjectDir[c], TargetDirName))
        .InitValue(ProjectId, projectId)
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