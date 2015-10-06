using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bud.Tasking.ApiV1 {
  public static class TasksExtensions {
    /// <param name="tasks">the tasks collection to which to add the given task modification.</param>
    /// <param name="taskModification">an incremental modification of a task.</param>
    /// <returns>a copy of these tasks with the added task modification.</returns>
    public static Tasks Add(this Tasks tasks, ITaskModification taskModification)
      => new Tasks(tasks.Concat(new[] {taskModification}));

    /// <summary>Defines a task that returns a constant value.</summary>
    /// <typeparam name="TResult">the type of the task's result</typeparam>
    /// <param name="tasks">the collection of tasks to which to add this constant.</param>
    /// <param name="taskName">the name of the task.</param>
    /// <param name="value">the constant value that should become the result of the task.</param>
    /// <remarks>this method discards any existing definitions of the task.</remarks>
    /// <returns>a copy of <paramref name="tasks" /> with the new definition of the task.</returns>
    public static Tasks Const<TResult>(this Tasks tasks, Key<TResult> taskName, TResult value)
      => tasks.Set(taskName, tsks => Task.FromResult(value));

    /// <summary>
    ///   Defines a task that returns the given constant <paramref name="value" /> (if the task is not defined yet).
    /// </summary>
    /// <remarks>this method does not override existing definitions of the task.</remarks>
    /// <returns>a copy of <paramref name="tasks" /> with the new definition of the task.</returns>
    public static Tasks InitConst<TResult>(this Tasks tasks, Key<TResult> taskName, TResult value)
      => tasks.Init(taskName, tsks => Task.FromResult(value));

    /// <summary>
    ///   Defines a task that returns the given constant <paramref name="value" /> (if the task is not defined yet).
    /// </summary>
    /// <remarks>this method does not override existing definitions of the task.</remarks>
    /// <returns>a copy of <paramref name="tasks" /> with the new definition of the task.</returns>
    public static Tasks Init<TResult>(this Tasks tasks, Key<TResult> taskName, Func<ITasks, Task<TResult>> value)
      => new Tasks(tasks.Add(new InitializeTask<TResult>(taskName, value)));

    /// <typeparam name="TResult">the type of the task's result</typeparam>
    /// <param name="tasks">the collection of tasks to which to add this constant.</param>
    /// <param name="taskName">the name of the task.</param>
    /// <param name="value">the constant value that should become the result of the task.</param>
    /// <remarks>this method overrides any existing definitions of the task.</remarks>
    /// <returns>a copy of <paramref name="tasks" /> with the new definition of the task.</returns>
    public static Tasks Set<TResult>(this Tasks tasks, Key<TResult> taskName, Func<ITasks, Task<TResult>> value)
      => new Tasks(tasks.Add(new SetTask<TResult>(taskName, value)));

    /// <typeparam name="TResult">the return type of the task.</typeparam>
    /// <returns>a copy of these tasks containing the new definition of the task.</returns>
    /// <remarks>
    ///   <para>
    ///     If the task with the given name is already defined, then the second parameter to the <paramref name="task" />
    ///     function will be non-<c>null</c> and can be used to modify the previous value of the task.
    ///   </para>
    ///   <para>
    ///     If the task has not been defined yet, then the second parameter to the <paramref name="task" />
    ///     function will be <c>null</c>.
    ///   </para>
    /// </remarks>
    public static Tasks Modify<TResult>(this Tasks tasks, Key<TResult> taskName, Func<ITasks, Task<TResult>, Task<TResult>> task)
      => new Tasks(tasks.Add(new ModifyTask<TResult>(taskName, task)));

    /// <returns>a copy of these tasks with added task definitions from <paramref name="tasks" />.</returns>
    public static Tasks ExtendWith(this Tasks tasks, Tasks otherTasks)
      => new Tasks(tasks.Concat(otherTasks));

    /// <returns>a copy of these tasks where the name of every task is prefixed with the given string and a slash character.</returns>
    public static Tasks Nest(this Tasks tasks, string prefix)
      => new Tasks(tasks.Select(taskModification => new TaskNesting(prefix, taskModification)));

    /// <param name="tasks"></param>
    /// <returns>a dictionary of task names mapped to task definitions.</returns>
    /// <remarks>
    ///   the task definitions are a result of the aggregation of all task modifications with the same name into task
    ///   definitions.
    /// </remarks>
    public static IDictionary<string, TaskDefinition> Compile(this Tasks tasks) {
      var taskDefinitions = new Dictionary<string, TaskDefinition>();
      foreach (var taskModification in tasks) {
        TaskDefinition taskDefinition;
        if (taskDefinitions.TryGetValue(taskModification.Name, out taskDefinition)) {
          taskDefinitions[taskModification.Name] = taskModification.Modify(taskDefinition);
        } else {
          taskDefinitions.Add(taskModification.Name, taskModification.ToTaskDefinition());
        }
      }
      return taskDefinitions;
    }

    /// <summary>
    ///   Asynchronously invokes the task with the given name and returns the result of the task.
    /// </summary>
    public static Task Get(this Tasks tasks, Key taskName) => tasks.ToResultCachingTasks().Get(taskName);

    /// <summary>
    ///   Asynchronously invokes the task with the given name and returns the typed result of the task.
    /// </summary>
    /// <exception cref="TaskReturnTypeException">
    ///   thrown if the actual type of the task does not
    ///   match the requested type of the task.
    /// </exception>
    /// <typeparam name="T">the requested type of the task's result.</typeparam>
    public static Task<T> Get<T>(this Tasks tasks, Key<T> taskName) => tasks.ToResultCachingTasks().Get(taskName);

    public static ResultCachingTasks ToResultCachingTasks(this Tasks tasks) => new ResultCachingTasks(tasks.Compile());
  }
}