using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Bud.Tasking {
  public class TaskerTest {
    private readonly TaskDefinitions taskDefinitions = TaskDefinitions.Empty;
    private Mock<Func<IContext, Task<string>>> fooTask;
    private Mock<Func<IContext, Task<string>, Task<string>>> appendBarStringTask;
    private Mock<Func<IContext, Task<string>>> mooTask;
    private readonly Func<IContext, Task<string>> duplicateFooTask = async context => await context.Invoke<string>("fooTask") + await context.Invoke<string>("fooTask");
    private readonly Func<IContext, Task<string>> inconsistentDependentTaskCalls = async context => await context.Invoke<string>("fooTask") + await context.Invoke<int>("fooTask");
    private readonly Func<IContext, Task<string>, Task<string>> appendFooModifier = async (context, oldValue) => await context.Invoke<string>("fooTask") + await oldValue;
    private readonly Func<IContext, Task<string>> multithreadedTask = async context => {
      var first = Task.Run(async () => {
        return await context.Invoke<string>("fooTask");
      });
      var second = Task.Run(async () => {
        return await context.Invoke<string>("fooTask");
      });
      return await first + await second;
    };

    [SetUp]
    public void SetUp() {
      fooTask = new Mock<Func<IContext, Task<string>>>();
      appendBarStringTask = new Mock<Func<IContext, Task<string>, Task<string>>>();
      mooTask = new Mock<Func<IContext, Task<string>>>();
      fooTask.Setup(self => self(It.IsAny<IContext>())).Returns(Task.FromResult("foo"));
      mooTask.Setup(self => self(It.IsAny<IContext>())).Returns(Task.FromResult("moo"));
      appendBarStringTask.Setup(self => self(It.IsAny<IContext>(), It.IsAny<Task<string>>())).Returns<IContext, Task<string>>(async (context, oldValue) => await oldValue + "bar");
    }

    [Test]
    public void Invoking_an_undefined_task_must_throw_an_exception() {
      var actualException = Assert.Throws<TaskUndefinedException>(async () => await Tasker.Invoke<string>(taskDefinitions, "fooTask"));
      Assert.AreEqual("Task 'fooTask' is undefined.", actualException.Message);
    }

    [Test]
    public async void invoking_an_async_task_must_return_value() {
      var fooTaskDefinition = taskDefinitions.SetAsync("fooTask", fooTask.Object);
      var taskValue = await Tasker.Invoke<string>(fooTaskDefinition, "fooTask");
      Assert.AreEqual("foo", taskValue);
    }

    [Test]
    public async void invoking_an_overridden_task_should_call_the_override_only_once() {
      var fooTaskDefinition = taskDefinitions.SetAsync("fooTask", fooTask.Object)
                                             .SetAsync("fooTask", fooTask.Object);
      await Tasker.Invoke<string>(fooTaskDefinition, "fooTask");
      fooTask.Verify(self => self(It.IsAny<IContext>()), Times.Once);
    }

    [Test]
    public async void invoking_modified_tasks_must_return_the_modified_value() {
      var fooTaskDefinition = taskDefinitions.SetAsync("fooTask", fooTask.Object)
                                             .ModifyAsync("fooTask", appendBarStringTask.Object);
      Assert.AreEqual("foobar", await Tasker.Invoke<string>(fooTaskDefinition, "fooTask"));
    }

    [Test]
    public void Invoking_a_valued_task_not_of_the_expected_type_must_throw_an_exception() {
      var fooTaskDefinition = taskDefinitions.SetAsync("fooTask", fooTask.Object);
      var actualException = Assert.Throws<TaskReturnsDifferentTypeException>(async () => await Tasker.Invoke<int>(fooTaskDefinition, "fooTask"));
      Assert.AreEqual("Task 'fooTask' returns 'System.String' but was expected to return 'System.Int32'.", actualException.Message);
    }

    [Test]
    public async void Defining_and_then_invoking_two_tasks_must_return_both_of_their_values() {
      var multipleTasks = taskDefinitions.SetAsync("fooTask", fooTask.Object)
                                         .SetAsync("mooTask", mooTask.Object);
      Assert.AreEqual("moo", await Tasker.Invoke<string>(multipleTasks, "mooTask"));
      Assert.AreEqual("foo", await Tasker.Invoke<string>(multipleTasks, "fooTask"));
    }

    [Test]
    public async void invoking_a_single_task_must_not_invoke_the_other() {
      var multipleTasks = taskDefinitions.SetAsync("fooTask", fooTask.Object)
                                         .SetAsync("mooTask", mooTask.Object);
      await Tasker.Invoke<string>(multipleTasks, "fooTask");
      mooTask.Verify(self => self(It.IsAny<IContext>()), Times.Never);
    }

    [Test]
    public async void Invoking_an_overridden_task_must_return_the_last_value() {
      var overriddenTaskDefinition = taskDefinitions.SetAsync("fooTask", fooTask.Object)
                                                    .SetAsync("fooTask", mooTask.Object);
      Assert.AreEqual("moo", await Tasker.Invoke<string>(overriddenTaskDefinition, "fooTask"));
    }

    [Test]
    public async void invoking_an_overriden_task_must_not_invoke_the_original_task() {
      var overriddenTaskDefinition = taskDefinitions.SetAsync("fooTask", fooTask.Object)
                                                    .SetAsync("fooTask", mooTask.Object);
      await Tasker.Invoke<string>(overriddenTaskDefinition, "fooTask");
      fooTask.Verify(self => self(It.IsAny<IContext>()), Times.Never);
    }

    [Test]
    public async void a_task_can_invoke_a_dependent_task() {
      var newTaskDefinitions = taskDefinitions.SetAsync("fooTask", fooTask.Object)
                                              .ModifyAsync("fooTask", appendBarStringTask.Object)
                                              .SetAsync("myTask", duplicateFooTask);
      Assert.AreEqual("foobarfoobar", await Tasker.Invoke<string>(newTaskDefinitions, "myTask"));
    }

    [Test]
    public async void a_dependent_task_is_invoked_only_once() {
      var newTaskDefinitions = taskDefinitions.SetAsync("fooTask", fooTask.Object)
                                              .SetAsync("myTask", duplicateFooTask);
      await Tasker.Invoke<string>(newTaskDefinitions, "myTask");
      fooTask.Verify(self => self(It.IsAny<IContext>()), Times.Once);
    }

    [Test]
    public void throw_when_invoking_a_dependent_task_for_the_second_time_expecting_the_wrong_type() {
      var newTaskDefinitions = taskDefinitions.SetAsync("fooTask", fooTask.Object)
                                              .SetAsync("myTask", inconsistentDependentTaskCalls);
      var exception = Assert.Throws<TaskReturnsDifferentTypeException>(async () => await Tasker.Invoke<string>(newTaskDefinitions, "myTask"));
      Assert.That(exception.Message, Contains.Substring("fooTask"));
      Assert.That(exception.Message, Contains.Substring("System.String"));
      Assert.That(exception.Message, Contains.Substring("System.Int32"));
    }

    [Test]
    public async void a_modifier_task_can_call_dependent_tasks() {
      var newTaskDefinitions = taskDefinitions.SetAsync("fooTask", fooTask.Object)
                                              .SetAsync("mooTask", mooTask.Object)
                                              .ModifyAsync("mooTask", appendFooModifier);
      Assert.AreEqual("foomoo", await Tasker.Invoke<string>(newTaskDefinitions, "mooTask"));
    }

    [Test]
    public async void multithreaded_access_to_tasks_should_prevent_duplicate_invocations() {
      var newTaskDefinitions = taskDefinitions.SetAsync("fooTask", fooTask.Object)
                                              .ModifyAsync("fooTask", appendBarStringTask.Object)
                                              .SetAsync("multithreadedTask", multithreadedTask);
      int repeatCount = 10;
      for (int i = 0; i < repeatCount; i++) {
        await Tasker.Invoke<string>(newTaskDefinitions, "multithreadedTask");
      }
      fooTask.Verify(self => self(It.IsAny<IContext>()), Times.Exactly(repeatCount));
      appendBarStringTask.Verify(self => self(It.IsAny<IContext>(), It.IsAny<Task<string>>()), Times.Exactly(repeatCount));
    }
  }
}