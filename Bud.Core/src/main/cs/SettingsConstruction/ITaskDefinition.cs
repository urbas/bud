using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Bud.SettingsConstruction {
  public interface ITaskDefinition {
    ImmutableHashSet<TaskKey> Dependencies { get; }
    Task Evaluate(IContext context, Key taskKey);
    ITaskDefinition WithDependencies(IEnumerable<TaskKey> newDependencies);
    Task EvaluateGuarded(IContext context, Key key, SemaphoreSlim semaphore, Func<Task> cachedValueGetter, Action<Task> valueCacher);
    object ExtractResult(Task completedTask);
  }
}