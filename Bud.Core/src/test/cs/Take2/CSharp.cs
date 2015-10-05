using System.IO;
using System.Threading.Tasks;
using Bud.Tasking;
using static Bud.Take2.Build;
using static Bud.Tasking.Tasks;

namespace Bud.Take2 {
  public static class CSharp {
    public static readonly Key<string> OutputAssembly = "outputAssembly";

    public static Tasks CSharpCompiler() => NewTasks.Set(OutputAssembly, GetOutputAssemblyPath);

    private static async Task<string> GetOutputAssemblyPath(ITasks tasks, Task<string> oldTask) {
      var baseDir = await ProjectDir[tasks];
      var id = await ProjectId[tasks];
      return Path.Combine(baseDir, "target", $"{id}.exe");
    }
  }
}