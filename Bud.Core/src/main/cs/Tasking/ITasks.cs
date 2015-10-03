using System.Threading.Tasks;

namespace Bud.Tasking {
  public interface ITasks {
    Task<T> Get<T>(Key<T> taskName);
    Task Get(string taskName);
  }
}