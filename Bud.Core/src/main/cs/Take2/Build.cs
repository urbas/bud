using Bud.Tasking;
using static Bud.Tasking.Tasks;

namespace Bud.Take2 {
  public static class Build {
    public static readonly Key<string> ProjectDir = "projectDir";
    public static readonly Key<string> ProjectId = "projectId";

    public static Tasks Project(string projectDir, string projectId) {
      return NewTasks.Const(ProjectId, projectId)
                     .Const(ProjectDir, projectDir);
    }
  }
}