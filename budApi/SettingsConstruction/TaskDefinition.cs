using System;

namespace Bud.SettingsConstruction {
  public static class TaskDefinition {

    public static ITaskDefinition<TResult> Create<TResult>(Func<BuildConfiguration, TResult> taskFunction) {
      return new GenericTaskDefinition<TResult>(taskFunction);
    }

    public static ITaskDefinition<TResult> Create<TResult>(Func<TResult> taskFunction) {
      return new TaskDefinition<TResult>(taskFunction);
    }

    public static ITaskDefinition<TResult> Create<TDependency1, TResult>(ValuedKey<TDependency1> dependency1, Func<TDependency1, TResult> taskFunction) {
      return new TaskDefinition<TResult, TDependency1>(dependency1, taskFunction);
    }
  }

  public interface ITaskDefinition<out TResult> {
    TResult Evaluate(BuildConfiguration buildConfiguration);
  }

  public class NoOpTask : ITaskDefinition<Unit> {
    public static readonly NoOpTask Instance = new NoOpTask();

    public Unit Evaluate(BuildConfiguration buildConfiguration) {
      return Unit.Instance;
    }
  }

  public class GenericTaskDefinition<TResult> : ITaskDefinition<TResult> {
    public readonly Func<BuildConfiguration, TResult> TaskFunction;

    public GenericTaskDefinition(Func<BuildConfiguration, TResult> taskFunction) {
      this.TaskFunction = taskFunction;
    }

    public TResult Evaluate(BuildConfiguration buildConfiguration) {
      return TaskFunction(buildConfiguration);
    }
  }

  public class TaskDefinition<TResult> : ITaskDefinition<TResult> {
    public readonly Func<TResult> TaskFunction;

    public TaskDefinition(Func<TResult> taskFunction) {
      this.TaskFunction = taskFunction;
    }

    public TResult Evaluate(BuildConfiguration buildConfiguration) {
      return TaskFunction();
    }
  }

  public class TaskDefinition<TResult, TDependency1> : ITaskDefinition<TResult> {
    public readonly IValuedKey<TDependency1> Dependency1;
    public readonly Func<TDependency1, TResult> TaskFunction;

    public TaskDefinition(IValuedKey<TDependency1> dependency1, Func<TDependency1, TResult> taskFunction) {
      this.TaskFunction = taskFunction;
      this.Dependency1 = dependency1;
    }

    public TResult Evaluate(BuildConfiguration buildConfiguration) {
      return TaskFunction(buildConfiguration.Evaluate(Dependency1));
    }
  }

}

