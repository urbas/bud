using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NUnit.Framework;

namespace Bud.Configuration {
  public class RawConfTest {
    private readonly ConfDefinition<int> confDefinition42 = new ConfDefinition<int>(_ => 42);
    private readonly ConfDefinition<object> confDefinitionNull = new ConfDefinition<object>(_ => null);

    [Test]
    public void Get_throws_an_exception_when_key_is_undefined() {
      Assert.Throws<ConfUndefinedException>(() => {
        var emptyConf = new RawConf(ImmutableDictionary<string, IConfDefinition>.Empty);
        emptyConf.Get<int>("A");
      });
    }

    [Test]
    public void Get_returns_the_defined_value()
      => Assert.AreEqual(42, ConfWithA42().Get<int>("A"));

    [Test]
    public void Get_throws_when_the_requested_type_mismatches_the_defined_type()
      => Assert.Throws<InvalidCastException>(() => ConfWithA42().Get<string>("A"));

    [Test]
    public void Get_returns_null() {
      var conf = new RawConf(new Dictionary<string, IConfDefinition> {
        {"A", confDefinitionNull}
      });
      Assert.IsNull(conf.Get<object>("A"));
    }

    [Test]
    public void TryGet_returns_empty_optional() {
      var conf = new RawConf(ImmutableDictionary<string, IConfDefinition>.Empty);
      Assert.IsFalse(conf.TryGet<object>("A").HasValue);
    }

    [Test]
    public void TryGet_returns_an_optional_with_a_value_when_the_key_is_defined()
      => Assert.IsTrue(ConfWithA42().TryGet<int>("A").HasValue);

    [Test]
    public void TryGet_returns_an_optional_containing_the_value_of_the_key()
      => Assert.AreEqual(42, ConfWithA42().TryGet<int>("A").Value);

    private RawConf ConfWithA42()
      => new RawConf(new Dictionary<string, IConfDefinition> {
        {"A", confDefinition42}
      });
  }
}