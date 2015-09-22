using System.Threading.Tasks;

namespace Bud.Tasking {
  public interface IContext {
    Task<T> Invoke<T>(string taskName);
  }
}