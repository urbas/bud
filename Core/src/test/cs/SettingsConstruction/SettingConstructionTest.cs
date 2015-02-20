using System;
using NUnit.Framework;

namespace Bud {
  public class SettingConstructionTest {
    private static readonly ConfigKey<string> TestKey = ConfigKey<string>.Define("testKey");
    private static readonly TaskKey<string> TestTaskKey = TaskKey<string>.Define("testTaskKey");
    private static readonly TaskKey<string> TestTaskKey2 = TaskKey<string>.Define("testTaskKey2");
    private static readonly TaskKey<string> TestTaskKey3 = TaskKey<string>.Define("testTaskKey3");

    [Test]
    public void Evaluating_an_initialized_config_MUST_return_the_value_of_initialization() {
      var settings = Settings.Create(TestKey.Init("foo"));
      Assert.AreEqual("foo", Context.FromSettings(settings).Evaluate(TestKey));
    }

    [Test]
    public void Evaluating_a_config_WHEN_initialization_is_performed_the_second_time_MUST_return_the_value_of_initialization() {
      var settings = Settings.Create(
        TestKey.Init("bar"),
        TestKey.Init("foo")
        );
      Assert.AreEqual("bar", Context.FromSettings(settings).Evaluate(TestKey));
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public void Modifying_an_uninitialised_config_MUST_throw_an_exception() {
      Context.FromSettings(Settings.Create(TestKey.Modify(v => v)));
    }

    [Test]
    public void Modifying_an_initialized_config_MUST_return_the_modified_value() {
      var settings = Settings.Create(
        TestKey.Init("foo"),
        TestKey.Modify((string v) => v + "bar"));
      Assert.AreEqual("foobar", Context.FromSettings(settings).Evaluate(TestKey));
    }

    [Test]
    public async void Evaluating_an_initialized_task_MUST_invoke_the_task_of_initialization() {
      var settings = Settings.Create(TestTaskKey.InitSync("foo"));
      Assert.AreEqual("foo", await Context.FromSettings(settings).Evaluate(TestTaskKey));
    }

    [Test]
    public async void Init_MUST_keep_the_value_of_the_first_initialization() {
      var settings = Settings.Create(TestTaskKey.InitSync("boo"), TestTaskKey.InitSync("foo"));
      Assert.AreEqual("boo", await Context.FromSettings(settings).Evaluate(TestTaskKey));
    }

    [Test]
    public async void Init_MUST_keep_the_value_of_the_first_ensure_initialization() {
      var settings = Settings.Create(TestTaskKey.InitSync("boo"), TestTaskKey.InitSync("foo"));
      Assert.AreEqual("boo", await Context.FromSettings(settings).Evaluate(TestTaskKey));
    }

    [Test]
    public async void Init_MUST_set_the_value() {
      var settings = Settings.Create(TestTaskKey.InitSync("foo"));
      Assert.AreEqual("foo", await Context.FromSettings(settings).Evaluate(TestTaskKey));
    }

    [Test]
    public async void Modifying_MUST_change_the_task() {
      var settings = Settings.Create(TestTaskKey.InitSync("foo"), TestTaskKey.Modify(async (b, prevTask) => await prevTask() + "bar"));
      Assert.AreEqual("foobar", await Context.FromSettings(settings).Evaluate(TestTaskKey));
    }

    [Test]
    public void AddDependencies_MUST_invoke_the_dependent_tasks() {
      bool wasDependentInvoked = false;
      var settings = Settings.Create(
        TestTaskKey.InitSync("foo"),
        TestTaskKey2.InitSync(context => {
          wasDependentInvoked = true;
          return "bar";
        }),
        TestTaskKey.DependsOn(TestTaskKey2));
      Context.FromSettings(settings).Evaluate(TestTaskKey);
      Assert.IsTrue(wasDependentInvoked);
    }

    [Test]
    public void AddDependencies_MUST_invoke_the_dependent_task_only_once() {
      int numberOfTimesDependentInvoked = 0;
      var settings = Settings.Create(
        TestTaskKey.InitSync(context => {
          ++numberOfTimesDependentInvoked;
          return "foo";
        }),
        TestTaskKey2.InitSync("bar"),
        TestTaskKey2.DependsOn(TestTaskKey),
        TestTaskKey3.InitSync("zar"),
        TestTaskKey3.DependsOn(TestTaskKey2, TestTaskKey));
      Context.FromSettings(settings).Evaluate(TestTaskKey3);
      Assert.AreEqual(1, numberOfTimesDependentInvoked);
    }

    [Test]
    public async void AddDependencies_MUST_invoke_the_dependent_task_only_once_WHEN_tasks_are_also_evaluated_in_the_tasks_body() {
      int numberOfTimesDependentInvoked = 0;
      var settings = Settings.Create(
        TestTaskKey.InitSync(() => {
          ++numberOfTimesDependentInvoked;
          return "foo";
        }),
        TestTaskKey2.Init(async context => await context.Evaluate(TestTaskKey) + "bar"),
        TestTaskKey2.DependsOn(TestTaskKey),
        TestTaskKey3.Init(async context => await context.Evaluate(TestTaskKey) + await context.Evaluate(TestTaskKey2) + "zar"),
        TestTaskKey3.DependsOn(TestTaskKey2, TestTaskKey));
      var evaluatedValue = await Context.FromSettings(settings).Evaluate(TestTaskKey3);
      Assert.AreEqual("foofoobarzar", evaluatedValue);
      Assert.AreEqual(1, numberOfTimesDependentInvoked);
    }

    [Test]
    public async void Modify_MUST_invoke_the_previous_task_only_once_WHEN_the_new_task_tries_to_invoke_it_twice() {
      int numberOfTimesDependentInvoked = 0;

      var settings = Settings.Create(
        TestTaskKey.InitSync(() => {
          ++numberOfTimesDependentInvoked;
          return "foo";
        }),
        TestTaskKey.Modify(async previousTask => await previousTask() + await previousTask()));

      var evaluatedValue = await Context.FromSettings(
        settings).Evaluate(TestTaskKey);

      Assert.AreEqual("foofoo", evaluatedValue);
      Assert.AreEqual(1, numberOfTimesDependentInvoked);
    }
  }
}