using System;
using System.Threading.Tasks;

namespace Bud.Tasking {
  public static class TasksExtensions {
    /// <summary>
    /// Adds a defininition of a task that returns a constant value.
    /// </summary>
    /// <typeparam name="TResult">the task's result</typeparam>
    /// <param name="tasks"></param>
    /// <param name="taskName">the name of the task.</param>
    /// <param name="value">the constant value that should become the result of the task.</param>
    /// <remarks>this method overrides definitions of </remarks>
    public static Tasks Const<TResult>(this Tasks tasks, Key<TResult> taskName, TResult value) {
      return tasks.Set(taskName, (tsks, oldTask) => Task.FromResult(value));
    }

    public static Tasks Init<TResult>(this Tasks tasks, Key<TResult> taskName, Func<ITasks, Task<TResult>> value) {
      return tasks.Set(taskName, (tsks, oldTask) => value(tsks));
    }

    internal static void AssertTaskTypeIsSame<T>(string taskName, Type returnType) {
      if (returnType != typeof(T)) {
        throw new TaskReturnTypeException($"Could not treat the type of task '{taskName}' as '{returnType}'. Its actual type is '{typeof(T)}'.");
      }
    }
  }
}