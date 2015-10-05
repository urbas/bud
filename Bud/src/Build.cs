using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bud.Tasking;

namespace Bud {
  public static class Build {
    public static readonly Key<string> ProjectDir = "projectDir";
    public static readonly Key<string> ProjectId = "projectId";
    public static readonly Key<IEnumerable<string>> Sources = "sources";

    public static Tasks Project(string projectDir, string projectId)
      => Tasks.NewTasks.Const(ProjectId, projectId)
                 .Const(ProjectDir, projectDir);

    public static Tasks SourceFiles(string fileFilter)
      => Tasks.NewTasks.Init(Sources, tasks => GetSources(tasks, fileFilter));

    private static async Task<IEnumerable<string>> GetSources(ITasks tasks, string searchPattern) {
      var sourceDir = Path.Combine(await ProjectDir[tasks], "src");
      return Directory.EnumerateFiles(sourceDir, searchPattern, SearchOption.AllDirectories);
    }
  }
}