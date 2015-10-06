using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Tasking;
using Bud.Tasking.ApiV1;
using static System.IO.Directory;
using static System.IO.Path;
using static Bud.Tasking.ApiV1.Tasks;

namespace Bud {
  public static class Build {
    public static readonly Key<string> ProjectDir = "projectDir";
    public static readonly Key<string> ProjectId = "projectId";
    public static readonly Key<IEnumerable<SourceDir>> SourceDirs = "sourceDirs";
    public static readonly Key<IEnumerable<string>> SourceFiles = "sourceFiles";

    public static Tasks Project(string projectDir, string projectId = null)
      => NewTasks.Const(ProjectId, projectId ?? GetFileName(projectDir))
                 .Const(ProjectDir, projectDir);

    public static Tasks Sources(string subfolder = null, string fileFilter = "*") {
      return NewTasks.Init(SourceFiles, GetSourceFiles)
                     .InitConst(SourceDirs, Enumerable.Empty<SourceDir>())
                     .Modify(SourceDirs, async (tasks, oldTask) => {
                       var sourceDir = subfolder == null ? await ProjectDir[tasks] : Combine(await ProjectDir[tasks], subfolder);
                       return (await oldTask).Concat(new[] {new SourceDir(sourceDir, fileFilter)});
                     });
    }

    private static async Task<IEnumerable<string>> GetSourceFiles(ITasks tasks) {
      return (await SourceDirs[tasks]).SelectMany(dir => EnumerateFiles(dir.Dir, dir.FileFilter, SearchOption.AllDirectories));
    }
  }

  public class SourceDir {
    public string Dir { get; }
    public string FileFilter { get; }

    public SourceDir(string dir, string fileFilter) {
      Dir = dir;
      FileFilter = fileFilter;
    }
  }
}