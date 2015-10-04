using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Bud.Tasking {
  public class TasksTest {
    private Mock<Func<ITasks, Task<string>>> fooTask;
    private Mock<Func<ITasks, Task<string>, Task<string>>> appendBarStringTask;
    private Mock<Func<ITasks, Task<string>>> mooTask;

    [SetUp]
    public void SetUp() {
      fooTask = new Mock<Func<ITasks, Task<string>>>();
      appendBarStringTask = new Mock<Func<ITasks, Task<string>, Task<string>>>();
      mooTask = new Mock<Func<ITasks, Task<string>>>();
      fooTask.Setup(self => self(It.IsAny<ITasks>())).Returns(Task.FromResult("foo"));
      mooTask.Setup(self => self(It.IsAny<ITasks>())).Returns(Task.FromResult("moo"));
      appendBarStringTask.Setup(self => self(It.IsAny<ITasks>(), It.IsAny<Task<string>>())).Returns<ITasks, Task<string>>(async (context, oldValue) => await oldValue + "bar");
    }

    [Test]
    public void overriding_a_task_with_a_different_type_must_throw_an_exception() {
      var exception = Assert.Throws<TaskReturnTypeException>(
        () => Tasks.New.SetAsync("fooTask", context => Task.FromResult("foo"))
                   .SetAsync("fooTask", context => Task.FromResult(42))
                   .Compile());
      Assert.That(exception.Message, Contains.Substring("fooTask"));
      Assert.That(exception.Message, Contains.Substring("System.Int32"));
      Assert.That(exception.Message, Contains.Substring("System.String"));
    }

    [Test]
    public void modifying_a_task_with_a_different_type_must_throw_an_exception() {
      var exception = Assert.Throws<TaskReturnTypeException>(
        () => Tasks.New.SetAsync("fooTask", context => Task.FromResult("foo"))
                   .ModifyAsync<int>("fooTask", (context, oldTask) => Task.FromResult(42))
                   .Compile());
      Assert.That(exception.Message, Contains.Substring("fooTask"));
      Assert.That(exception.Message, Contains.Substring("System.String"));
      Assert.That(exception.Message, Contains.Substring("System.Int32"));
    }

    [Test]
    public void throw_when_modifying_a_task_that_has_not_yet_been_defined() {
      var exception = Assert.Throws<TaskUndefinedException>(
        () => Tasks.New.ModifyAsync<int>("fooTask", (context, oldTask) => Task.FromResult(42))
                   .Compile());
      Assert.That(exception.Message, Contains.Substring("fooTask"));
    }

    [Test]
    public void ExtendedWith_must_contain_task_definitions_from_the_original_as_well_as_extending_tasks() {
      var originalTasks = Tasks.New.SetAsync("fooTask", tasks => Task.FromResult(42));
      var extendingTasks = Tasks.New.SetAsync("barTask", tasks => Task.FromResult(52));
      var combinedTasks = originalTasks.ExtendWith(extendingTasks).Compile();
      Assert.IsTrue(combinedTasks.ContainsKey("fooTask"));
      Assert.IsTrue(combinedTasks.ContainsKey("barTask"));
    }

    [Test]
    public void TryGetTask_must_return_false_when_the_task_is_not_defined() {
      TaskDefinition task;
      Assert.IsFalse(Tasks.New.Compile().TryGetValue("fooTask", out task));
      Assert.IsNull(task);
    }

    [Test]
    public void TryGetTask_must_return_true_and_set_the_out_parameter_to_the_given_task() {
      TaskDefinition taskDefinition;
      Func<ITasks, Task<int>> expectedTask = ctxt => Task.FromResult(42);
      var tasks = Tasks.New.SetAsync("fooTask", expectedTask);
      Assert.IsTrue(tasks.Compile().TryGetValue("fooTask", out taskDefinition));
      Assert.AreSame(typeof(int), taskDefinition.ReturnType);
      Assert.AreSame(expectedTask, taskDefinition.Task);
    }

    [Test]
    public void Invoking_an_undefined_task_must_throw_an_exception() {
      var actualException = Assert.Throws<TaskUndefinedException>(async () => await Tasks.New.Get<string>("fooTask"));
      Assert.AreEqual("Task 'fooTask' is undefined.", actualException.Message);
    }

    [Test]
    public async void Invoking_an_async_task_must_return_value() {
      var taskResult = await Tasks.New.SetAsync("fooTask", fooTask.Object)
                                  .Get<string>("fooTask");
      Assert.AreEqual("foo", taskResult);
    }

    [Test]
    public async void Invoking_an_overridden_task_should_call_the_override_only_once() {
      await Tasks.New.SetAsync("fooTask", fooTask.Object)
                 .SetAsync("fooTask", fooTask.Object)
                 .Get<string>("fooTask");
      fooTask.Verify(self => self(It.IsAny<ITasks>()), Times.Once);
    }

    [Test]
    public async void Invoking_modified_tasks_must_return_the_modified_value() {
      var taskResult = await Tasks.New.SetAsync("fooTask", fooTask.Object)
                                  .ModifyAsync("fooTask", appendBarStringTask.Object)
                                  .Get<string>("fooTask");
      Assert.AreEqual("foobar", taskResult);
    }

    [Test]
    public void Invoking_a_task_expecting_the_wrong_result_type_must_throw_an_exception() {
      var tasks = Tasks.New.SetAsync("fooTask", fooTask.Object);
      var actualException = Assert.Throws<TaskReturnTypeException>(async () => await tasks.Get<int>("fooTask"));
      Assert.AreEqual("Task 'fooTask' returns 'System.String' but was expected to return 'System.Int32'.", actualException.Message);
    }

    [Test]
    public async void Defining_and_then_invoking_two_tasks_must_return_both_of_their_values() {
      var tasks = Tasks.New.SetAsync("fooTask", fooTask.Object)
                       .SetAsync("mooTask", mooTask.Object);
      Assert.AreEqual("moo", await tasks.Get<string>("mooTask"));
      Assert.AreEqual("foo", await tasks.Get<string>("fooTask"));
    }

    [Test]
    public async void Invoking_a_single_task_must_not_invoke_the_other() {
      await Tasks.New.SetAsync("fooTask", fooTask.Object)
                 .SetAsync("mooTask", mooTask.Object)
                 .Get<string>("fooTask");
      mooTask.Verify(self => self(It.IsAny<ITasks>()), Times.Never);
    }

    [Test]
    public async void Invoking_an_overridden_task_must_return_the_last_value() {
      var taskResult = await Tasks.New.SetAsync("fooTask", fooTask.Object)
                                  .SetAsync("fooTask", mooTask.Object)
                                  .Get<string>("fooTask");
      Assert.AreEqual("moo", taskResult);
    }

    [Test]
    public async void Invoking_an_overriden_task_must_not_invoke_the_original_task() {
      await Tasks.New.SetAsync("fooTask", fooTask.Object)
                 .SetAsync("fooTask", mooTask.Object)
                 .Get<string>("fooTask");
      fooTask.Verify(self => self(It.IsAny<ITasks>()), Times.Never);
    }

    [Test]
    public async void a_task_can_invoke_a_dependent_task() {
      var taskResult = await Tasks.New.SetAsync("fooTask", fooTask.Object)
                                  .ModifyAsync("fooTask", appendBarStringTask.Object)
                                  .SetAsync("myTask", InvokeFooTwiceAndConcatenate)
                                  .Get<string>("myTask");
      Assert.AreEqual("foobarfoobar", taskResult);
    }

    [Test]
    public async void a_dependent_task_is_invoked_only_once() {
      await Tasks.New.SetAsync("fooTask", fooTask.Object)
                 .SetAsync("myTask", InvokeFooTwiceAndConcatenate)
                 .Get<string>("myTask");
      fooTask.Verify(self => self(It.IsAny<ITasks>()), Times.Once);
    }

    [Test]
    public void throw_when_invoking_a_dependent_task_for_the_second_time_expecting_the_wrong_type() {
      var tasks = Tasks.New.SetAsync("fooTask", fooTask.Object)
                       .SetAsync("myTask", InvokeFooAsStringAndInt);
      var exception = Assert.Throws<TaskReturnTypeException>(async () => await tasks.Get<string>("myTask"));
      Assert.That(exception.Message, Contains.Substring("fooTask"));
      Assert.That(exception.Message, Contains.Substring("System.String"));
      Assert.That(exception.Message, Contains.Substring("System.Int32"));
    }

    [Test]
    public async void a_modifier_task_can_call_dependent_tasks() {
      var taskResult = await Tasks.New.SetAsync("fooTask", fooTask.Object)
                                  .SetAsync("mooTask", mooTask.Object)
                                  .ModifyAsync<string>("mooTask", PrependFooTaskResult)
                                  .Get<string>("mooTask");
      Assert.AreEqual("foomoo", taskResult);
    }

    [Test]
    public async void multithreaded_access_to_tasks_should_prevent_duplicate_invocations() {
      var tasks = Tasks.New.SetAsync("fooTask", fooTask.Object)
                       .ModifyAsync("fooTask", appendBarStringTask.Object)
                       .SetAsync("multithreadedTask", InvokeFooTaskTwiceConcurrently);
      int repeatCount = 10;
      for (int i = 0; i < repeatCount; i++) {
        await tasks.Get<string>("multithreadedTask");
      }
      fooTask.Verify(self => self(It.IsAny<ITasks>()), Times.Exactly(repeatCount));
      appendBarStringTask.Verify(self => self(It.IsAny<ITasks>(), It.IsAny<Task<string>>()), Times.Exactly(repeatCount));
    }

    [Test]
    public async void Invoking_a_task_without_specifying_a_type() {
      await Tasks.New.SetAsync("fooTask", fooTask.Object)
                 .Get("fooTask");
      fooTask.Verify(self => self(It.IsAny<ITasks>()));
    }

    [Test]
    public async void invoke_once_when_invoking_a_task_without_specifying_a_type_and_again_with_a_type() {
      var tasks = Tasks.New.SetAsync("fooTask", fooTask.Object)
                       .SetAsync("myTask", InvokeFooTwiceTypedAndUntyped);
      await tasks.Get("myTask");
      fooTask.Verify(self => self(It.IsAny<ITasks>()));
    }

    [Test]
    public async void invoking_a_constant_task() {
      var tasks = Tasks.New.Const("fooTask", 42);
      Assert.AreEqual(42, await tasks.Get<int>("fooTask"));
    }

    [Test]
    public async void invoking_a_synchronous_task() {
      var tasks = Tasks.New.Set("fooTask", () => 42);
      Assert.AreEqual(42, await tasks.Get<int>("fooTask"));
    }

    [Test]
    public async void invoking_a_synchronous_modified_task() {
      var tasks = Tasks.New.Set("fooTask", () => 42)
                       .Modify<int>("fooTask", oldTaskValue => oldTaskValue + 58);
      Assert.AreEqual(100, await tasks.Get<int>("fooTask"));
    }

    [Test]
    public async void nesting_prefixes_task_names() {
      var tasks = Tasks.New.Const("fooTask", 42);
      var nestedTasks = Tasks.New.Nest("bar", tasks);
      Assert.AreEqual(42, await nestedTasks.Get<int>("bar/fooTask"));
    }

    [Test]
    public async void nesting_allows_access_to_sibling_tasks() {
      var tasks = Tasks.New.SetAsync("fooTask", fooTask.Object)
                       .SetAsync("duplicatingTask", InvokeFooTwiceAndConcatenate);
      var nestedTasks = Tasks.New.Nest("bar", tasks);
      Assert.AreEqual("foofoo", await nestedTasks.Get<string>("bar/duplicatingTask"));
      fooTask.Verify(self => self(It.IsAny<ITasks>()));
    }

    [Test]
    public void throw_when_modifying_a_nested_undefined_task() {
      var tasks = Tasks.New.Modify<int>("fooTask", oldTaskValue => oldTaskValue + 1);
      var nestedTasks = Tasks.New.Nest("bar", tasks);
      var exception = Assert.Throws<TaskUndefinedException>(async () => await nestedTasks.Get<int>("bar/fooTask"));
      Assert.That(exception.Message, Contains.Substring("bar/fooTask"));
    }

    private static async Task<string> InvokeFooTwiceAndConcatenate(ITasks context) {
      return await context.Get<string>("fooTask") + await context.Get<string>("fooTask");
    }

    private static async Task<string> InvokeFooTwiceTypedAndUntyped(ITasks tasks) {
      await tasks.Get("fooTask");
      return await tasks.Get<string>("fooTask");
    }

    private static async Task<string> InvokeFooTaskTwiceConcurrently(ITasks context) {
      var first = Task.Run(async () => await context.Get<string>("fooTask"));
      var second = Task.Run(async () => await context.Get<string>("fooTask"));
      return await first + await second;
    }

    private static async Task<string> InvokeFooAsStringAndInt(ITasks context) {
      return await context.Get<string>("fooTask") + await context.Get<int>("fooTask");
    }

    private static async Task<string> PrependFooTaskResult(ITasks context, Task<string> oldValue) {
      return await context.Get<string>("fooTask") + await oldValue;
    }
  }
}