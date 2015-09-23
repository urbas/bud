using System.Threading.Tasks;
using NUnit.Framework;

namespace Bud.Tasking {
  public class TasksTest {
    [Test]
    public void overriden_a_task_with_a_different_type_must_throw_an_exception() {
      var exception = Assert.Throws<TaskTypeOverrideException>(
        () => Tasks.Empty.SetAsync("fooTask", context => Task.FromResult("foo"))
                   .SetAsync("fooTask", context => Task.FromResult(42)));
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
  }
}