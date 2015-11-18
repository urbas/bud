using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NUnit.Framework;

namespace Bud.Configuration {
  public class ConfValueCalculatorTest {
    private readonly ConfDefinition<int> confDefinition42 = new ConfDefinition<int>(_ => 42, ImmutableList<string>.Empty);
    private readonly ConfDefinition<object> confDefinitionNull = new ConfDefinition<object>(_ => null, ImmutableList<string>.Empty);

    [Test]
    public void Get_throws_an_exception_when_key_is_undefined() {
      Assert.Throws<ConfUndefinedException>(() => {
        var emptyConf = new ConfValueCalculator(ImmutableDictionary<string, IConfDefinition>.Empty);
        emptyConf.Get<int>("A", null);
      });
    }

    [Test]
    public void Get_returns_the_defined_value() {
      var conf = new ConfValueCalculator(new Dictionary<string, IConfDefinition> {
        {"A", confDefinition42}
      });
      Assert.AreEqual(42, conf.Get<int>("A", null));
    }

    [Test]
    public void Get_throws_when_the_requested_type_mismatches_the_defined_type() {
      Assert.Throws<InvalidCastException>(() => {
        var conf = new ConfValueCalculator(new Dictionary<string, IConfDefinition> {
          {"A", confDefinition42}
        });
        conf.Get<string>("A", null);
      });
    }

    [Test]
    public void Get_returns_null() {
      var conf = new ConfValueCalculator(new Dictionary<string, IConfDefinition> {
        {"A", confDefinitionNull}
      });
      Assert.IsNull(conf.Get<object>("A", null));
    }
  }
}