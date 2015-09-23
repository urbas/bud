using System.Threading.Tasks;

namespace Bud.Tasking {
  public class Tasker {
    public static async Task<T> Invoke<T>(Tasks tasks, string taskName) {
      return await new Context(tasks).Invoke<T>(taskName);
    }
  }
}