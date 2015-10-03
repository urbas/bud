using Bud.Tasking;

namespace Bud.Take2 {
  public static class Build {
    public static readonly Key<string> ProjectDir = "projectDir";
    public static readonly Key<string> ProjectId = "projectId";

    public static Tasks Project(string projectDir, string projectId) {
      return Tasks.New.Const(ProjectDir, projectDir)
                  .Const(ProjectId, projectId);
    }
  }
}