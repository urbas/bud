using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using static Bud.Tasking.Tasks;

namespace Bud.Tasking {
  public class TasksTest {
    private Mock<Func<ITasks, Task<int>, Task<int>>> mockTask;
    private Mock<Func<Task<int>, int>> mockIncrementContinuation;
    private readonly Key<int> fooInt = "fooTask";
    private readonly Key<int> barInt = "barTask";
    private readonly Key<string> fooString = "fooTask";

    [SetUp]
    public void SetUp() {
      mockTask = new Mock<Func<ITasks, Task<int>, Task<int>>>();
      mockTask.Setup(self => self(It.IsAny<ITasks>(), It.IsAny<Task<int>>()))
              .Returns<ITasks, Task<int>>(SetTo42OrIncrement);
      mockIncrementContinuation = new Mock<Func<Task<int>, int>>();
      mockIncrementContinuation.Setup(self => self(It.IsAny<Task<int>>()))
                               .Returns((Task<int> task) => task.Result + 1);
    }

    [Test]
    public void Overriding_a_task_with_a_different_type_must_throw_an_exception() {
      var exception = Assert.Throws<TaskReturnTypeException>(
        () => NewTasks.Set(fooString, (tasks, oldTask) => Task.FromResult("foo"))
                      .Set(fooInt, (tasks, oldTask) => Task.FromResult(42))
                      .Compile());
      Assert.That(exception.Message, Contains.Substring("fooTask"));
      Assert.That(exception.Message, Contains.Substring("System.Int32"));
      Assert.That(exception.Message, Contains.Substring("System.String"));
    }

    [Test]
    public async void Invoke_a_task() {
      Assert.AreEqual(42, await NewTasks.Set(fooInt, SetTo42OrIncrement).Get(fooInt));
    }

    [Test]
    public async void Invoke_a_task_without_specifying_a_type() {
      await NewTasks.Set(fooInt, mockTask.Object).Get(fooInt.Id);
      mockTask.Verify(self => self(It.IsAny<ITasks>(), It.IsAny<Task<int>>()));
    }

    [Test]
    public async void Invoke_a_modified_task() {
      var tasks = NewTasks.Set(fooInt, SetTo42OrIncrement)
                          .Set(fooInt, SetTo42OrIncrement);
      Assert.AreEqual(43, await tasks.Get(fooInt));
    }

    [Test]
    public async void Defining_and_then_invoking_two_tasks_must_return_both_of_their_values() {
      var tasks = NewTasks.Set(fooInt, mockTask.Object)
                          .Set(barInt, (tsks, oldTask) => Task.FromResult(1337));
      Assert.AreEqual(42, await tasks.Get(fooInt));
      Assert.AreEqual(1337, await tasks.Get(barInt));
    }

    [Test]
    public async void Invoking_a_single_task_must_not_invoke_the_other() {
      var tasks = NewTasks.Set(fooInt, mockTask.Object)
                          .Set(barInt, (tsks, oldTask) => Task.FromResult(1337));
      await tasks.Get(barInt);
      mockTask.Verify(self => self(It.Is<ITasks>(t => t == tasks), It.Is<Task<int>>(t => t == null)), Times.Never);
    }

    [Test]
    public void Extended_tasks_must_contain_task_definitions_from_the_original_as_well_as_extending_tasks() {
      var originalTasks = NewTasks.Set(fooInt, (tasks, task) => Task.FromResult(42));
      var extendingTasks = NewTasks.Set(barInt, (tasks, task) => Task.FromResult(52));
      var combinedTasks = originalTasks.ExtendWith(extendingTasks).Compile();
      Assert.IsTrue(combinedTasks.ContainsKey(fooInt));
      Assert.IsTrue(combinedTasks.ContainsKey(barInt));
    }

    [Test]
    public void Compiled_tasks_must_not_contain_undefined_tasks() {
      TaskDefinition task;
      Assert.IsFalse(NewTasks.Compile().TryGetValue(fooInt, out task));
      Assert.IsNull(task);
    }

    [Test]
    public void Invoking_an_undefined_task_must_throw_an_exception() {
      var actualException = Assert.Throws<TaskUndefinedException>(async () => await NewTasks.Get(fooString));
      Assert.That(actualException.Message, Contains.Substring(fooInt));
    }

    [Test]
    public async void Compiled_tasks_must_contain_task_definitions() {
      TaskDefinition taskDefinition;
      var tasks = NewTasks.Set(fooInt, mockTask.Object);
      Assert.IsTrue(tasks.Compile().TryGetValue(fooInt, out taskDefinition));
      Assert.AreEqual(typeof(int), taskDefinition.ReturnType);
      Assert.AreEqual(42, await (Task<int>) taskDefinition.Task(tasks));
      mockTask.Verify(self => self(It.Is<ITasks>(t => t == tasks), It.Is<Task<int>>(t => t == null)));
    }

    [Test]
    public void Invoking_a_task_expecting_the_wrong_result_type_must_throw_an_exception() {
      var exception = Assert.Throws<TaskReturnTypeException>(async () => await NewTasks.Set(fooInt, mockTask.Object).Get(fooString));
      Assert.That(exception.Message, Contains.Substring(fooInt));
      Assert.That(exception.Message, Contains.Substring("System.Int32"));
      Assert.That(exception.Message, Contains.Substring("System.String"));
    }

    [Test]
    public void Do_not_await_the_original_task_when_the_modification_ignores_the_original() {
      NewTasks.Set(fooInt, async (tsks, task) => await mockTask.Object(tsks, task))
              .Set(fooInt, (tsks, oldTask) => Task.FromResult(1337))
              .Get(fooInt);
      mockIncrementContinuation.Verify(self => self(It.IsAny<Task<int>>()), Times.Never);
    }

    [Test]
    public async void A_task_can_invoke_another_task() {
      var taskResult = await NewTasks.Set(fooInt, mockTask.Object)
                                     .Set(barInt, AddFooTwice)
                                     .Get(barInt);
      Assert.AreEqual(84, taskResult);
    }

    [Test]
    public async void Tasks_are_invoked_once_only_and_their_result_is_cached() {
      await NewTasks.Set(fooInt, mockTask.Object)
                    .Set(barInt, AddFooTwice)
                    .Get(barInt);
      mockTask.Verify(self => self(It.IsAny<ITasks>(), It.IsAny<Task<int>>()));
    }

    [Test]
    public async void Multithreaded_access_to_tasks_should_not_result_in_duplicate_invocations() {
      var tasks = NewTasks.Set(fooInt, mockTask.Object)
                          .Set(barInt, AddFooTwiceConcurrently);
      int repeatCount = 10;
      for (int i = 0; i < repeatCount; i++) {
        await tasks.Get(barInt);
      }
      mockTask.Verify(self => self(It.IsAny<ITasks>(), It.IsAny<Task<int>>()), Times.Exactly(repeatCount));
    }

    [Test]
    public async void Nesting_prefixes_task_names() {
      var nestedTasks = NewTasks.Set(fooInt, mockTask.Object).Nest("bar");
      Assert.AreEqual(42, await nestedTasks.Get("bar" / fooInt));
    }

    [Test]
    public async void Nesting_allows_access_to_sibling_tasks() {
      var nestedTasks = NewTasks.Set(fooInt, mockTask.Object)
                                .Set(barInt, AddFooTwice)
                                .Nest("bar");
      Assert.AreEqual(84, await nestedTasks.Get("bar" / barInt));
      mockTask.Verify(self => self(It.IsAny<ITasks>(), It.IsAny<Task<int>>()));
    }

    private async Task<int> SetTo42OrIncrement(ITasks context, Task<int> oldTask) {
      return oldTask == null ? 42 : (await oldTask.ContinueWith(mockIncrementContinuation.Object));
    }

    private async Task<int> AddFooTwice(ITasks tasks, Task<int> oldTask) {
      return await tasks.Get(fooInt) + await tasks.Get(fooInt);
    }

    private async Task<int> AddFooTwiceConcurrently(ITasks tsks, Task<int> oldTask) {
      var first = Task.Run(async () => await tsks.Get(fooInt));
      var second = Task.Run(async () => await tsks.Get(fooInt));
      return await first + await second;
    }
  }
}