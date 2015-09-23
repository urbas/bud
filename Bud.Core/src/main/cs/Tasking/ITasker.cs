using System.Threading.Tasks;

namespace Bud.Tasking {
  public interface ITasker {
    Task<T> Invoke<T>(string taskName);
  }
}