using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using static Bud.Tasking.ApiV1.Tasks;

namespace Bud.Tasking.ApiV1 {
  public class TasksExtensionsTest {
    private readonly Key<int> fooInt = "fooTask";
    private readonly Key<int> barInt = "barTask";
    private readonly Key<Task<int>> barAsyncInt = "barAsyncTask";
    private readonly Key<string> fooString = "fooTask";

    [Test]
    public void NewTasks_is_empty() => Assert.IsEmpty(NewTasks);

    [Test]
    public void Add_must_append_the_task_modification_to_tasks() {
      var taskModification = new Mock<ITaskModification>().Object;
      Assert.That(NewTasks.Add(taskModification), Contains.Item(taskModification));
    }

    [Test]
    public void Const_defines_a_task_that_returns_the_constant_value() {
      Assert.AreEqual(42, NewTasks.Const(fooInt, 42).Get(fooInt));
    }

    [Test]
    public void Const_redefines_tasks() {
      Assert.AreEqual(1, NewTasks.Const(fooInt, 42).Const(fooInt, 1).Get(fooInt));
    }

    [Test]
    public void InitConst_defines_a_task_that_returns_the_constant_value() {
      Assert.AreEqual(42, NewTasks.InitConst(fooInt, 42).Get(fooInt));
    }

    [Test]
    public void InitConst_does_not_redefine_tasks() {
      Assert.AreEqual(42, NewTasks.InitConst(fooInt, 42).InitConst(fooInt, 1).Get(fooInt));
    }

    [Test]
    public void Redefined_task_is_never_invoked() {
      var intTask = new Mock<Func<ITasks, int>>();
      Assert.AreEqual(1, NewTasks.Init(fooInt, intTask.Object).Const(fooInt, 1).Get(fooInt));
      intTask.Verify(self => self(It.IsAny<ITasks>()), Times.Never());
    }

    [Test]
    public void Modified_task_is_never_invoked() {
      var taskResult = NewTasks.InitConst(fooInt, 42).Modify(fooInt, (tasks, task) => task + 1).Get(fooInt);
      Assert.AreEqual(43, taskResult);
    }

    [Test]
    public void Throw_when_modiying_a_task_that_does_not_yet_exist() {
      var taskResult = Assert.Throws<TaskDefinitionException>(
        () => NewTasks.Modify(fooInt, (tasks, task) => task + 1).Get(fooInt));
      Assert.AreEqual(fooInt.Id, taskResult.TaskName);
      Assert.AreEqual(fooInt.Type, taskResult.ResultType);
    }

    [Test]
    public void Throw_when_requiring_the_wrong_task_result_type() {
      var exception = Assert.Throws<TaskReturnTypeException>(
        () => NewTasks.Const(fooString, "foo").Get(fooInt));
      Assert.That(exception.Message, Contains.Substring("fooTask"));
      Assert.That(exception.Message, Contains.Substring("System.Int32"));
      Assert.That(exception.Message, Contains.Substring("System.String"));
    }

    [Test]
    public void Defining_and_then_invoking_two_tasks_must_return_both_of_their_values() {
      var tasks = NewTasks.Const(fooInt, 42).Const(barInt, 1337);
      Assert.AreEqual(42, tasks.Get(fooInt));
      Assert.AreEqual(1337, tasks.Get(barInt));
    }

    [Test]
    public void Invoking_a_single_task_must_not_invoke_the_other() {
      var intTask = new Mock<Func<ITasks, int>>();
      var tasks = NewTasks.Set(fooInt, intTask.Object).Const(barInt, 1337);
      tasks.Get(barInt);
      intTask.Verify(self => self(It.IsAny<ITasks>()), Times.Never);
    }

    [Test]
    public void Extended_tasks_must_contain_task_definitions_from_the_original_as_well_as_extending_tasks() {
      var originalTasks = NewTasks.Const(fooInt, 42);
      var extendingTasks = NewTasks.Const(barInt, 58);
      var combinedTasks = originalTasks.ExtendWith(extendingTasks);
      Assert.AreEqual(42, combinedTasks.Get(fooInt));
      Assert.AreEqual(58, combinedTasks.Get(barInt));
    }

    [Test]
    public void Invoking_an_undefined_task_must_throw_an_exception() {
      var actualException = Assert.Throws<TaskUndefinedException>(() => NewTasks.Get(fooString));
      Assert.That(actualException.Message, Contains.Substring(fooInt));
    }

    [Test]
    public void A_task_can_invoke_another_task() {
      var taskResult = NewTasks.Const(fooInt, 42)
                               .Set(barInt, tasks => tasks.Get(fooInt) + 1)
                               .Get(barInt);
      Assert.AreEqual(43, taskResult);
    }

    [Test]
    public void Tasks_are_invoked_once_only_and_their_result_is_cached() {
      var intTask = new Mock<Func<ITasks, int>>();
      NewTasks.Set(fooInt, intTask.Object)
              .Set(barInt, tasks => fooInt[tasks] + fooInt[tasks])
              .Get(barInt);
      intTask.Verify(self => self(It.IsAny<ITasks>()));
    }

    [Test]
    public async void Multithreaded_access_to_tasks_should_not_result_in_duplicate_invocations() {
      var intTask = new Mock<Func<ITasks, int>>();
      var tasks = NewTasks.Set(fooInt, intTask.Object)
                          .Set(barAsyncInt, AddFooTwiceConcurrently);
      int repeatCount = 10;
      for (int i = 0; i < repeatCount; i++) {
        await tasks.Get(barAsyncInt);
      }
      intTask.Verify(self => self(It.IsAny<ITasks>()), Times.Exactly(repeatCount));
    }

    [Test]
    public void Nesting_prefixes_task_names() {
      var nestedTasks = NewTasks.Const(fooInt, 42).Nest("bar");
      Assert.AreEqual(42, nestedTasks.Get("bar" / fooInt));
    }

    [Test]
    public void Nesting_allows_access_to_sibling_tasks() {
      var nestedTasks = NewTasks.Set(barInt, tasks => fooInt[tasks] + 1)
                                .Const(fooInt, 42)
                                .Nest("bar");
      Assert.AreEqual(43, nestedTasks.Get("bar" / barInt));
    }

    private async Task<int> AddFooTwiceConcurrently(ITasks tsks) {
      var first = Task.Run(() => tsks.Get(fooInt));
      var second = Task.Run(() => tsks.Get(fooInt));
      return await first + await second;
    }
  }
}