namespace Bud.Tasking.ApiV1 {
  public class NestedTasks : ITasks {
    private readonly string prefix;
    private readonly ITasks tasks;

    public NestedTasks(string prefix, ITasks tasks) {
      this.prefix = prefix + "/";
      this.tasks = tasks;
    }

    public T Get<T>(Key<T> taskName) => tasks.Get<T>(prefix + taskName);
  }
}