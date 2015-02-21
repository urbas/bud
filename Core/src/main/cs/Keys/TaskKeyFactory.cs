using System.Collections.Immutable;

namespace Bud.Keys {
  internal sealed class TaskKeyFactory : IKeyFactory<TaskKey> {
    public static readonly IKeyFactory<TaskKey> Instance = new TaskKeyFactory();

    private TaskKeyFactory() {}

    public TaskKey Define(ImmutableList<string> path, string description) {
      return new TaskKey(path, description);
    }
  }

  internal sealed class TaskKeyFactory<T> : IKeyFactory<TaskKey<T>> {
    public static readonly IKeyFactory<TaskKey<T>> Instance = new TaskKeyFactory<T>();

    private TaskKeyFactory() {}

    public TaskKey<T> Define(ImmutableList<string> path, string description) {
      return new TaskKey<T>(path, description);
    }
  }
}