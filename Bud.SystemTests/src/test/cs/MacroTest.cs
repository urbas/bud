using Bud.Commander;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class MacroTest {
    [Test]
    public void Macros_MUST_change_settings_AND_return_values() {
      using (var buildCommander = TestProjects.Load(this)) {
        Assert.AreEqual("Hello, BuildContext and MacroResult World!", buildCommander.EvaluateMacro<string>("mostGeneralDefinitionMacro", "strange"));
        Assert.AreEqual("strange", buildCommander.Evaluate<string>("introducedConfig"));

        buildCommander.EvaluateMacroToJson("settingsModifierMacro");
        Assert.AreEqual("Foo bar value.", buildCommander.Evaluate<string>("introducedConfig2"));

        Assert.AreEqual("Hello, BuildContext Macro World!", buildCommander.EvaluateMacro<string>("valueReturningMacro"));
      }
    }
  }
}