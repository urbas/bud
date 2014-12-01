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
      var buildConfiguration = Settings.Start
        .Initialize(TestKey, "foo");
      Assert.AreEqual("foo", buildConfiguration.ToEvaluationContext().Evaluate(TestKey));
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Initializing_the_same_config_twice_MUST_throw_an_exception() {
      Settings.Start
        .Initialize(TestKey, "bar")
        .Initialize(TestKey, "foo")
        .ToEvaluationContext();
    }

    [Test]
    public void Evaluating_a_config_WHEN_ensure_initialized_is_performed_after_initialization_MUST_return_the_value_of_initialization() {
      var buildConfiguration = Settings.Start
        .Initialize(TestKey, "bar")
        .EnsureInitialized(TestKey, "foo");
      Assert.AreEqual("bar", buildConfiguration.ToEvaluationContext().Evaluate(TestKey));
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Modifying_an_uninitialised_config_MUST_throw_an_exception() {
      Settings.Start.Modify(TestKey, v => v).ToEvaluationContext();
    }

    [Test]
    public void Modifying_an_initialized_config_MUST_return_the_modified_value() {
      var buildConfiguration = Settings.Start
        .Initialize(TestKey, "foo")
        .Modify(TestKey, v => v + "bar");
      Assert.AreEqual("foobar", buildConfiguration.ToEvaluationContext().Evaluate(TestKey));
    }

    [Test]
    public async void Evaluating_an_initialized_task_MUST_invoke_the_task_of_initialization() {
      var buildConfiguration = Settings.Start.Initialize(TestTaskKey, b => "foo");
      Assert.AreEqual("foo", await buildConfiguration.ToEvaluationContext().Evaluate(TestTaskKey));
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Initializing_a_task_twice_MUST_throw_an_exception() {
      Settings.Start.Initialize(TestTaskKey, b => "foo").Initialize(TestTaskKey, b => "boo").ToEvaluationContext();
    }

    [Test]
    public async void EnsureInitialized_MUST_keep_the_value_of_the_first_initialization() {
      var buildConfiguration = Settings.Start.Initialize(TestTaskKey, b => "boo").EnsureInitialized(TestTaskKey, b => "foo");
      Assert.AreEqual("boo", await buildConfiguration.ToEvaluationContext().Evaluate(TestTaskKey));
    }

    [Test]
    public async void EnsureInitialized_MUST_keep_the_value_of_the_first_ensure_initialization() {
      var buildConfiguration = Settings.Start.EnsureInitialized(TestTaskKey, b => "boo").EnsureInitialized(TestTaskKey, b => "foo");
      Assert.AreEqual("boo", await buildConfiguration.ToEvaluationContext().Evaluate(TestTaskKey));
    }

    [Test]
    public async void EnsureInitialized_MUST_set_the_value() {
      var buildConfiguration = Settings.Start.EnsureInitialized(TestTaskKey, b => "foo");
      Assert.AreEqual("foo", await buildConfiguration.ToEvaluationContext().Evaluate(TestTaskKey));
    }

    [Test]
    public async void Modifying_MUST_change_the_task() {
      var buildConfiguration = Settings.Start.Initialize(TestTaskKey, b => "foo").Modify(TestTaskKey, async (b, prevTask) => await prevTask() + "bar");
      Assert.AreEqual("foobar", await buildConfiguration.ToEvaluationContext().Evaluate(TestTaskKey));
    }

    [Test]
    public void AddDependencies_MUST_invoke_the_dependent_tasks() {
      bool wasDependentInvoked = false;
      var buildConfiguration = Settings.Start
        .Initialize(TestTaskKey, b => "foo")
        .Initialize(TestTaskKey2, b => {
        wasDependentInvoked = true;
        return "bar";
      })
        .AddDependencies(TestTaskKey, TestTaskKey2);
      buildConfiguration.ToEvaluationContext().Evaluate(TestTaskKey);
      Assert.IsTrue(wasDependentInvoked);
    }

    [Test]
    public void AddDependencies_MUST_invoke_the_dependent_task_only_once() {
      int numberOfTimesDependentInvoked = 0;
      var buildConfiguration = Settings.Start
        .Initialize(TestTaskKey, b => {
        ++numberOfTimesDependentInvoked;
        return "foo";
      })
        .Initialize(TestTaskKey2, b => "bar").AddDependencies(TestTaskKey2, TestTaskKey)
        .Initialize(TestTaskKey3, b => "zar").AddDependencies(TestTaskKey3, TestTaskKey2, TestTaskKey);
      buildConfiguration.ToEvaluationContext().Evaluate(TestTaskKey3);
      Assert.AreEqual(1, numberOfTimesDependentInvoked);
    }

    [Test]
    public async void AddDependencies_MUST_invoke_the_dependent_task_only_once_WHEN_tasks_are_also_evaluated_in_the_tasks_body() {
      int numberOfTimesDependentInvoked = 0;
      var buildConfiguration = Settings.Start
        .Initialize(TestTaskKey, b => {
        ++numberOfTimesDependentInvoked;
        return "foo";
      })
        .Initialize(TestTaskKey2, async b => await b.Evaluate(TestTaskKey) + "bar").AddDependencies(TestTaskKey2, TestTaskKey)
        .Initialize(TestTaskKey3, async b => await b.Evaluate(TestTaskKey) + await b.Evaluate(TestTaskKey2) + "zar").AddDependencies(TestTaskKey3, TestTaskKey2, TestTaskKey);
      var evaluatedValue = await buildConfiguration.ToEvaluationContext().Evaluate(TestTaskKey3);
      Assert.AreEqual("foofoobarzar", evaluatedValue);
      Assert.AreEqual(1, numberOfTimesDependentInvoked);
    }

    [Test]
    public async void Modify_MUST_invoke_the_previous_task_only_once_WHEN_the_new_task_tries_to_invoke_it_twice() {
      int numberOfTimesDependentInvoked = 0;

      var evaluatedValue = await Settings.Start
        .Initialize(TestTaskKey, context => {
        ++numberOfTimesDependentInvoked;
        return "foo";
      })
        .Modify(TestTaskKey, async (context, previousTask) => await previousTask() + await previousTask())
        .ToEvaluationContext().Evaluate(TestTaskKey);

      Assert.AreEqual("foofoo", evaluatedValue);
      Assert.AreEqual(1, numberOfTimesDependentInvoked);
    }
  }
}

