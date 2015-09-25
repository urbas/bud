using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bud.Tasking {
  public class TasksTest {
    [Test]
    public void overriding_a_task_with_a_different_type_must_throw_an_exception() {
      var exception = Assert.Throws<TaskTypeOverrideException>(
        () => Tasks.Empty.SetAsync("fooTask", context => Task.FromResult("foo"))
                   .SetAsync("fooTask", context => Task.FromResult(42)));
      Assert.That(exception.Message, Contains.Substring("fooTask"));
      Assert.That(exception.Message, Contains.Substring("System.String"));
      Assert.That(exception.Message, Contains.Substring("System.Int32"));
    }

    [Test]
    public void modifying_a_task_with_a_different_type_must_throw_an_exception() {
      var exception = Assert.Throws<TaskTypeOverrideException>(
        () => Tasks.Empty.SetAsync("fooTask", context => Task.FromResult("foo"))
                   .ModifyAsync<int>("fooTask", (context, oldTask) => Task.FromResult(42)));
      Assert.That(exception.Message, Contains.Substring("fooTask"));
      Assert.That(exception.Message, Contains.Substring("System.String"));
      Assert.That(exception.Message, Contains.Substring("System.Int32"));
    }

    [Test]
    public void throw_when_modifying_a_task_that_has_not_yet_been_defined() {
      var exception = Assert.Throws<TaskUndefinedException>(
        () => Tasks.Empty.ModifyAsync<int>("fooTask", (context, oldTask) => Task.FromResult(42)));
      Assert.That(exception.Message, Contains.Substring("fooTask"));
    }

    [Test]
    public void TryGetTask_must_return_false_when_the_task_is_not_defined() {
      ITaskDefinition task;
      Assert.IsFalse(Tasks.Empty.TryGetTask("fooTask", out task));
    }

    [Test]
    public void TryGetTask_must_return_true_and_set_the_out_parameter_to_the_given_task() {
      ITaskDefinition taskDefinition;
      Func<ITasker, Task<int>> expectedTask = ctxt => Task.FromResult(42);
      var tasks = Tasks.Empty.SetAsync("fooTask", expectedTask);
      Assert.IsTrue(tasks.TryGetTask("fooTask", out taskDefinition));
      Assert.AreSame(typeof(int), taskDefinition.ReturnType);
      Assert.AreSame(expectedTask, taskDefinition.Task);
    }
  }
}