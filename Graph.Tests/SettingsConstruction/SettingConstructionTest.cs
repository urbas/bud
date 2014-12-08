using System;
using Bud.SettingsConstruction;
using NUnit.Framework;

namespace Bud {
  public class SettingConstructionTest {

    private static readonly ConfigKey<string> TestKey = new ConfigKey<string>("testKey");
    private static readonly TaskKey<string> TestTaskKey = new TaskKey<string>("testTaskKey");
    private static readonly TaskKey<string> TestTaskKey2 = new TaskKey<string>("testTaskKey2");
    private static readonly TaskKey<string> TestTaskKey3 = new TaskKey<string>("testTaskKey3");

    [Test]
    public void Evaluating_an_initialized_config_MUST_return_the_value_of_initialization() {
      var settings = Settings.Empty
        .Init(TestKey, "foo");
      Assert.AreEqual("foo", EvaluationContext.FromSettings(settings).Evaluate(TestKey));
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Initializing_the_same_config_twice_MUST_throw_an_exception() {
      EvaluationContext.FromSettings(Settings.Empty
        .Init(TestKey, "bar")
        .Init(TestKey, "foo")
      );
    }

    [Test]
    public void Evaluating_a_config_WHEN_ensure_initialized_is_performed_after_initialization_MUST_return_the_value_of_initialization() {
      var settings = Settings.Empty
        .Init(TestKey, "bar")
        .InitOrKeep(TestKey, "foo");
      Assert.AreEqual("bar", EvaluationContext.FromSettings(settings).Evaluate(TestKey));
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Modifying_an_uninitialised_config_MUST_throw_an_exception() {
      EvaluationContext.FromSettings(Settings.Empty.Modify(TestKey, v => v));
    }

    [Test]
    public void Modifying_an_initialized_config_MUST_return_the_modified_value() {
      var settings = Settings.Empty
        .Init(TestKey, "foo")
        .Modify(TestKey, v => v + "bar");
      Assert.AreEqual("foobar", EvaluationContext.FromSettings(settings).Evaluate(TestKey));
    }

    [Test]
    public async void Evaluating_an_initialized_task_MUST_invoke_the_task_of_initialization() {
      var settings = Settings.Empty.Init(TestTaskKey, context => "foo");
      Assert.AreEqual("foo", await EvaluationContext.FromSettings(settings).Evaluate(TestTaskKey));
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Initializing_a_task_twice_MUST_throw_an_exception() {
      EvaluationContext.FromSettings(
        Settings.Empty.Init(TestTaskKey, context => "foo").Init(TestTaskKey, context => "boo")
      );
    }

    [Test]
    public async void EnsureInitialized_MUST_keep_the_value_of_the_first_initialization() {
      var settings = Settings.Empty.Init(TestTaskKey, context => "boo").InitOrKeep(TestTaskKey, context => "foo");
      Assert.AreEqual("boo", await EvaluationContext.FromSettings(settings).Evaluate(TestTaskKey));
    }

    [Test]
    public async void EnsureInitialized_MUST_keep_the_value_of_the_first_ensure_initialization() {
      var settings = Settings.Empty.InitOrKeep(TestTaskKey, context => "boo").InitOrKeep(TestTaskKey, context => "foo");
      Assert.AreEqual("boo", await EvaluationContext.FromSettings(settings).Evaluate(TestTaskKey));
    }

    [Test]
    public async void EnsureInitialized_MUST_set_the_value() {
      var settings = Settings.Empty.InitOrKeep(TestTaskKey, context => "foo");
      Assert.AreEqual("foo", await EvaluationContext.FromSettings(settings).Evaluate(TestTaskKey));
    }

    [Test]
    public async void Modifying_MUST_change_the_task() {
      var settings = Settings.Empty.Init(TestTaskKey, context => "foo").Modify(TestTaskKey, async (b, prevTask) => await prevTask() + "bar");
      Assert.AreEqual("foobar", await EvaluationContext.FromSettings(settings).Evaluate(TestTaskKey));
    }

    [Test]
    public void AddDependencies_MUST_invoke_the_dependent_tasks() {
      bool wasDependentInvoked = false;
      var settings = Settings.Empty
        .Init(TestTaskKey, context => "foo")
        .Init(TestTaskKey2, context => {
        wasDependentInvoked = true;
        return "bar";
      })
        .AddDependencies(TestTaskKey, TestTaskKey2);
      EvaluationContext.FromSettings(settings).Evaluate(TestTaskKey);
      Assert.IsTrue(wasDependentInvoked);
    }

    [Test]
    public void AddDependencies_MUST_invoke_the_dependent_task_only_once() {
      int numberOfTimesDependentInvoked = 0;
      var settings = Settings.Empty
        .Init(TestTaskKey, context => {
        ++numberOfTimesDependentInvoked;
        return "foo";
      })
        .Init(TestTaskKey2, context => "bar").AddDependencies(TestTaskKey2, TestTaskKey)
        .Init(TestTaskKey3, context => "zar").AddDependencies(TestTaskKey3, TestTaskKey2, TestTaskKey);
      EvaluationContext.FromSettings(settings).Evaluate(TestTaskKey3);
      Assert.AreEqual(1, numberOfTimesDependentInvoked);
    }

    [Test]
    public async void AddDependencies_MUST_invoke_the_dependent_task_only_once_WHEN_tasks_are_also_evaluated_in_the_tasks_body() {
      int numberOfTimesDependentInvoked = 0;
      var settings = Settings.Empty
        .Init(TestTaskKey, context => {
        ++numberOfTimesDependentInvoked;
        return "foo";
      })
        .Init(TestTaskKey2, async context => await context.Evaluate(TestTaskKey) + "bar").AddDependencies(TestTaskKey2, TestTaskKey)
        .Init(TestTaskKey3, async context => await context.Evaluate(TestTaskKey) + await context.Evaluate(TestTaskKey2) + "zar").AddDependencies(TestTaskKey3, TestTaskKey2, TestTaskKey);
      var evaluatedValue = await EvaluationContext.FromSettings(settings).Evaluate(TestTaskKey3);
      Assert.AreEqual("foofoobarzar", evaluatedValue);
      Assert.AreEqual(1, numberOfTimesDependentInvoked);
    }

    [Test]
    public async void Modify_MUST_invoke_the_previous_task_only_once_WHEN_the_new_task_tries_to_invoke_it_twice() {
      int numberOfTimesDependentInvoked = 0;

      var evaluatedValue = await EvaluationContext.FromSettings(Settings.Empty
        .Init(TestTaskKey, context => {
        ++numberOfTimesDependentInvoked;
        return "foo";
      })
        .Modify(TestTaskKey, async (context, previousTask) => await previousTask() + await previousTask())
      ).Evaluate(TestTaskKey);

      Assert.AreEqual("foofoo", evaluatedValue);
      Assert.AreEqual(1, numberOfTimesDependentInvoked);
    }
  }
}

