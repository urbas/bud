using Bud.IO;
using Bud.Tasking;
using Bud.Tasking.ApiV1;
using static System.IO.Path;
using static Bud.Tasking.ApiV1.Tasks;

namespace Bud {
  public static class Build {
    public static readonly Key<string> ProjectDir = "projectDir";
    public static readonly Key<string> ProjectId = "projectId";
    public static readonly Key<IFiles> SourceFiles = "sourceFiles";
    public static readonly Key<IFiles> TargetFiles = "targetFiles";

    public static Tasks Project(string projectDir, string projectId = null)
      => NewTasks.InitConst(SourceFiles, Files.Empty)
                 .InitConst(TargetFiles, Files.Empty)
                 .InitConst(ProjectId, projectId ?? GetFileName(projectDir))
                 .InitConst(ProjectDir, projectDir);

    public static Tasks Sources(string subfolder = null, string fileFilter = "*") {
      return NewTasks.Modify(SourceFiles, async (tasks, existingSources) => {
        var sourceDir = subfolder == null ? await ProjectDir[tasks] : Combine(await ProjectDir[tasks], subfolder);
        return (await existingSources).ExtendWith(sourceDir, fileFilter);
      });
    }
  }
}