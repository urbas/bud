using System.Threading.Tasks;

namespace Bud.Tasking {
  internal class PrefixedTasks : ITasks {
    private readonly string prefix;
    private readonly ITasks tasks;

    public PrefixedTasks(string prefix, ITasks tasks) {
      this.prefix = prefix + "/";
      this.tasks = tasks;
    }

    public Task<T> Get<T>(Key<T> taskName) => tasks.Get<T>(prefix + taskName);
    public Task Get(string taskName) => tasks.Get(prefix + taskName);
  }
}