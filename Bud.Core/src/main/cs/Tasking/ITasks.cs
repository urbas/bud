using System.Threading.Tasks;

namespace Bud.Tasking {
  public interface ITasks {
    Task<T> Invoke<T>(string taskName);
    Task Invoke(string taskName);
  }
}