using Bud.Commander;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class MacroTest {
    [Test]
    public void settingsModifierMacro_MUST_introduce_a_new_config_key() {
      using (var buildCommander = TestProjects.LoadBuildCommander(this)) {
        Assert.AreEqual("Hello, BuildContext and MacroResult World!", buildCommander.Evaluate<string>("@mostGeneralDefinitionMacro"));
        Assert.AreEqual("Something", buildCommander.Evaluate<string>("introducedConfig"));

        buildCommander.EvaluateToJson("@settingsModifierMacro");
        Assert.AreEqual("Foo bar value.", buildCommander.Evaluate<string>("introducedConfig2"));

        Assert.AreEqual("Hello, BuildContext Macro World!", buildCommander.Evaluate<string>("@valueReturningMacro"));
      }
    }
  }
}