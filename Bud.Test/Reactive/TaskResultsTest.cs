using System;
using System.Reactive;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using static System.Threading.Tasks.Task;
using static Bud.Reactive.TaskResults;
using static Bud.Util.Option;
using static NUnit.Framework.Assert;

namespace Bud.Reactive {
  public class TaskResultsTest {
    [Test]
    public void Await_returns_unit_when_given_null()
      => AreEqual(None<object>(), Await(null));

    [Test]
    public void Await_returns_unit_when_the_task_is_void()
      => AreEqual(Some<object>(Unit.Default),
                  Await(Run(new Mock<Action>().Object)));

    [Test]
    public void Await_invoked_the_action_in_a_void_task() {
      var actionMock = new Mock<Action>(MockBehavior.Strict);
      actionMock.Setup(s => s());
      Await(Run(actionMock.Object));
      actionMock.VerifyAll();
    }

    [Test]
    public void Await_returns_the_result_when_the_task_is_typed()
      => AreEqual(Some<object>(42),
                  Await(FromResult(42)));

    [Test]
    public void IsTaskWithResult_returns_false_when_given_null()
      => IsFalse(IsTaskWithResult(null));

    [Test]
    public void IsTaskWithResult_returns_false_when_given_void_task()
      => IsFalse(IsTaskWithResult(new Task(() => { })));

    [Test]
    public void IsTaskWithResult_returns_true_when_given_task_with_result()
      => IsTrue(IsTaskWithResult(FromResult("foo")));

    [Test]
    public void IsTaskWithResult_returns_true_when_given_composite_result_task()
      => IsTrue(IsTaskWithResult(new Task<int>(() => 42).ContinueWith(task => task.Result + 1)));
  }
}