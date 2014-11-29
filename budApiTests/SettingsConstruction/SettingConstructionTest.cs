using System;
using Bud.SettingsConstruction;
using NUnit.Framework;

namespace Bud {
  public class SettingConstructionTest {

    private static readonly ConfigKey<string> TestKey = new ConfigKey<string>("testKey");

    [Test]
    public void Evaluating_an_initialized_config_MUST_return_the_value_of_initialization() {
      var buildConfiguration = Settings.Start
        .Initialize(TestKey, "foo");
      Assert.AreEqual("foo", buildConfiguration.ToBuildConfiguration().Evaluate(TestKey));
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Initializing_the_same_config_twice_MUST_throw_an_exception() {
      Settings.Start
        .Initialize(TestKey, "bar")
        .Initialize(TestKey, "foo")
        .ToBuildConfiguration();
    }

    [Test]
    public void Evaluating_a_config_WHEN_ensure_initialized_is_performed_after_initialization_MUST_return_the_value_of_initialization() {
      var buildConfiguration = Settings.Start
        .Initialize(TestKey, "bar")
        .EnsureInitialized(TestKey, "foo");
      Assert.AreEqual("bar", buildConfiguration.ToBuildConfiguration().Evaluate(TestKey));
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Modifying_an_uninitialised_config_MUST_throw_an_exception() {
      Settings.Start.Modify(TestKey, v => v).ToBuildConfiguration();
    }

    [Test]
    public void Modifying_an_initialized_config_MUST_return_the_modified_value() {
      var buildConfiguration = Settings.Start
        .Initialize(TestKey, "foo")
        .Modify(TestKey, v => v + "bar");
      Assert.AreEqual("foobar", buildConfiguration.ToBuildConfiguration().Evaluate(TestKey));
    }

  }
}

