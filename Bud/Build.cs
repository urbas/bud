using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bud.Tasking;
using static Bud.Tasking.Tasks;

namespace Bud {
  public static class Build {
    public static readonly Key<string> ProjectDir = "projectDir";
    public static readonly Key<string> ProjectId = "projectId";
    public static readonly Key<IEnumerable<string>> Sources = "sources";

    public static Tasks Project(string projectDir, string projectId = null)
      => NewTasks.Const(ProjectId, projectId ?? Path.GetFileName(projectDir))
                 .Const(ProjectDir, projectDir);

    public static Tasks SourceFiles(string subfolder = null, string fileFilter = "*")
      => NewTasks.Init(Sources, tasks => GetSources(tasks, fileFilter, subfolder));

    private static async Task<IEnumerable<string>> GetSources(ITasks tasks, string searchPattern, string subfolder) {
      var sourceDir = subfolder == null ? await ProjectDir[tasks] : Path.Combine(await ProjectDir[tasks], subfolder);
      return Directory.EnumerateFiles(sourceDir, searchPattern, SearchOption.AllDirectories);
    }
  }
}