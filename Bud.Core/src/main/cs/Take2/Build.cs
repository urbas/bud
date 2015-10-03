using System.Threading.Tasks;
using Bud.Tasking;

namespace Bud.Take2 {
  public static class Build {
    public static Tasks Project(string projectDir, string projectId) {
      return Tasks.New.SetAsync("projectDir", tasker => Task.FromResult(projectDir))
                  .SetAsync("projectId", tasker => Task.FromResult(projectId));
    }
  }
}