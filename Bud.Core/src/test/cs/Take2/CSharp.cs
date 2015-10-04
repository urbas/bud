using System.IO;
using System.Threading.Tasks;
using Bud.Tasking;

namespace Bud.Take2 {
  public static class CSharp {
    public static readonly Key<string> OutputAssembly = "outputAssembly";

    public static Tasks CSharpProject() => Tasks.New.SetAsync(OutputAssembly, GetOutputAssemblyPath);

    private static async Task<string> GetOutputAssemblyPath(ITasks tasks) {
      var baseDir = await tasks.Get(Build.ProjectDir);
      var id = await tasks.Get(Build.ProjectId);
      return Path.Combine(baseDir, "target", $"{id}.exe");
    }
  }
}