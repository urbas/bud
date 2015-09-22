using System.Threading.Tasks;

namespace Bud.Tasking {
  public class Tasker {
    public static async Task<T> Invoke<T>(TaskDefinitions taskDefinitions, string taskName) {
      return await new Context(taskDefinitions).Invoke<T>(taskName);
    }
  }
}