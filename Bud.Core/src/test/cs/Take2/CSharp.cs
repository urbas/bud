using System.IO;
using System.Threading.Tasks;
using Bud.Tasking;

namespace Bud.Take2 {
  public static class CSharp {
    public static Tasks Project() => Tasks.New.SetAsync("outputAssembly", GetOutputAssemblyPath);

    private static async Task<string> GetOutputAssemblyPath(ITasks tasks) {
      var baseDir = await tasks.Invoke<string>("projectDir");
      var id = await tasks.Invoke<string>("projectId");
      return Path.Combine(baseDir, "target", $"{id}.exe");
    }
  }
}