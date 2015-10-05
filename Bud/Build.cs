using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bud.Tasking;
using static System.IO.Directory;
using static System.IO.Path;
using static Bud.Tasking.Tasks;

namespace Bud {
  public static class Build {
    public static readonly Key<string> ProjectDir = "projectDir";
    public static readonly Key<string> ProjectId = "projectId";
    public static readonly Key<IEnumerable<string>> Sources = "sources";

    public static Tasks Project(string projectDir, string projectId = null)
      => NewTasks.Const(ProjectId, projectId ?? GetFileName(projectDir))
                 .Const(ProjectDir, projectDir);

    public static Tasks SourceFiles(string subfolder = null, string fileFilter = "*") {
      return NewTasks.Set(Sources, async (tasks, oldTask) => {
        var sourceDir = subfolder == null ? await ProjectDir[tasks] : Combine(await ProjectDir[tasks], subfolder);
        var sourceFiles = EnumerateFiles(sourceDir, fileFilter, SearchOption.AllDirectories);
        return oldTask == null ? sourceFiles : (await oldTask).Concat(sourceFiles);
      });
    }
  }
}