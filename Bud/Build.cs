using Bud.IO;
using Bud.Tasking;
using Bud.Tasking.ApiV1;
using static System.IO.Path;
using static Bud.Tasking.ApiV1.Tasks;

namespace Bud {
  public static class Build {
    public static readonly Key<string> ProjectDir = "projectDir";
    public static readonly Key<string> ProjectId = "projectId";
    public static readonly Key<FilesObserver> SourceFiles = "sourceFiles";

    public static Tasks Project(string projectDir, string projectId = null)
      => NewTasks.Const(ProjectId, projectId ?? GetFileName(projectDir))
                 .Const(ProjectDir, projectDir);

    public static Tasks Sources(string subfolder = null, string fileFilter = "*") {
      return NewTasks.InitConst(SourceFiles, FilesObserver.Empty)
                     .Modify(SourceFiles, async (tasks, oldTask) => {
                       var sourceDir = subfolder == null ? await ProjectDir[tasks] : Combine(await ProjectDir[tasks], subfolder);
                       return (await oldTask).ExtendWith(sourceDir, fileFilter);
                     });
    }
  }
}