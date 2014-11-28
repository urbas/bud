using System;
using Bud.SettingsConstruction;
using NUnit.Framework;

namespace Bud {
  public class SettingConstructionTest {

    private static readonly ConfigKey<string> TestKey = new ConfigKey<string>();
    private Settings initializedTestKeySetting = Settings.Start.EnsureInitialized(TestKey, "foo");

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Modifying_an_uninitialised_key_MUST_throw_an_exception() {
      Settings.Start.Modify(TestKey, v => v).End();
    }

    [Test]
    public void Evaluating_an_initialized_key_MUST_return_the_value_of_initialization() {
      var buildConfiguration = initializedTestKeySetting.End();
      Assert.AreEqual("foo", buildConfiguration.Evaluate(TestKey));
    }

    [Test]
    public void Modifying_an_initialized_key_MUST_return_the_modified_value() {
      var buildConfiguration = initializedTestKeySetting.Modify(TestKey, v => v + "bar").End();
      Assert.AreEqual("foobar", buildConfiguration.Evaluate(TestKey));
    }

  }
}

