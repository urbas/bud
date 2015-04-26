using Bud.Commander;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class MacroTest {
    [Test]
    public void configKeyIntroductionMacro_MUST_introduce_a_new_config_key() {
      using (var buildCommander = TestProjects.LoadBuildCommander(this)) {
        buildCommander.EvaluateToJson("@configKeyIntroductionMacro");
        var introducedConfig = buildCommander.Evaluate<string>("introducedConfig");
        Assert.AreEqual("Foo bar value.", introducedConfig);
        Assert.AreEqual("Hello, Macro World!", buildCommander.Evaluate<string>("@helloMacro"));
        Assert.AreEqual("Hello, BuildContext Macro World!", buildCommander.Evaluate<string>("@helloBuildContextMacro"));
        Assert.AreEqual("Hello, BuildContext and MacroResult World!", buildCommander.Evaluate<string>("@helloBuildContextMacroResultMacro"));
      }
    }
  }
}