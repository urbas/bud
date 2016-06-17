using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Bud.Make {
  public class RuleTest {
    [Test]
    public void AddRule_returns_a_rule_collection_with_two_rules()
      => That(Rule1.Add(Rule1),
              Is.EqualTo(new[] {Rule1, Rule1}));

    private static Rule Rule1
      => Make.Rule("out1", (string input, string output) => {}, "input1");
  }
}