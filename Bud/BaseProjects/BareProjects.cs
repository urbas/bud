using System.IO;
using System.Reactive;
using Bud.V1;
using static System.IO.Directory;
using static Bud.V1.Api;
using static Bud.V1.ApiImpl;

namespace Bud.BaseProjects {
  internal static class BareProjects {
    internal static Conf CreateBareProject(string projectDir,
                                           string projectId,
                                           string version = DefaultVersion)
      => Project(projectId)
        .Add(BuildSchedulingSupport)
        .InitValue(ProjectId, projectId)
        .InitValue(ProjectDir, projectDir)
        .InitValue(Version, version)
        .Init(TargetDir, c => Path.Combine(ProjectDir[c], TargetDirName))
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