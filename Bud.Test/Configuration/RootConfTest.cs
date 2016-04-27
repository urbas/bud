using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.V1;
using NUnit.Framework;
using static NUnit.Framework.Assert;
using Contains = NUnit.Framework.Contains;

namespace Bud.Configuration {
  public class RootConfTest {
    private static readonly Exception ExceptionInA = new Exception("a");
    private readonly ConfDefinition<int> defInt42 = new ConfDefinition<int>(_ => 42);
    private readonly ConfDefinition<int> defIntCallsA = new ConfDefinition<int>(c => c.Get<int>("A"));
    private readonly ConfDefinition<object> defObjectNull = new ConfDefinition<object>(_ => null);

    private readonly ConfDefinition<int> defIntThrows = new ConfDefinition<int>(_ => {
      throw ExceptionInA;
    });

    [Test]
    public void Get_throws_an_exception_when_key_is_undefined() {
      Throws<ConfUndefinedException>(() => {
        var emptyConf = new RootConf(ImmutableDictionary<string, IConfDefinition>.Empty);
        emptyConf.Get<int>("A");
      });
    }

    [Test]
    public void Get_returns_the_defined_value()
      => AreEqual(42, ConfWithA42().Get<int>("A"));

    [Test]
    public void Get_throws_when_the_requested_type_mismatches_the_defined_type()
      => Throws<InvalidCastException>(() => ConfWithA42().Get<string>("A"));

    [Test]
    public void Get_returns_null() {
      var conf = new RootConf(new Dictionary<string, IConfDefinition> {
        {"A", defObjectNull}
      });
      IsNull(conf.Get<object>("A"));
    }

    [Test]
    public void TryGet_on_empty_returns_none() {
      var conf = new RootConf(ImmutableDictionary<string, IConfDefinition>.Empty);
      IsFalse(conf.TryGet<object>("A").HasValue);
    }

    [Test]
    public void TryGet_returns_an_optional_with_a_value_when_the_key_is_defined()
      => IsTrue(ConfWithA42().TryGet<int>("A").HasValue);

    [Test]
    public void TryGet_returns_an_optional_containing_the_value_of_the_key()
      => AreEqual(42, ConfWithA42().TryGet<int>("A").Value);

    [Test]
    public void Exception_message_contains_the_name_of_the_key_in_which_the_exception_was_produced() {
      var exception = Throws<ConfAccessException>(() => KeysWithExceptions().Get<int>("A"));
      That(exception.Message, Contains.Substring("'A'"));
    }

    [Test]
    public void Inner_exception_must_be_the_one_thrown_in_value_calculation() {
      var exception = Throws<ConfAccessException>(() => KeysWithExceptions().Get<int>("A"));
      AreEqual(ExceptionInA, exception.InnerException);
    }

    [Test]
    public void Inner_exception_message_must_contain_the_path_to_the_referenced_key_that_thre() {
      var exceptionB = Throws<ConfAccessException>(() => KeysWithExceptions().Get<int>("B"));
      That(exceptionB.Message, Contains.Substring("'A' referenced from 'B'"));
      AreEqual(ExceptionInA, exceptionB.InnerException);
    }

    [Test]
    public void Confs_can_retrieve_their_keys() {
      var conf = new RootConf(new Dictionary<string, IConfDefinition> {
        {"foo/A", new ConfDefinition<Key>(c => c.Key)}
      });
      AreEqual("foo/A", conf.Get<Key>("foo/A").Id);
    }

    private RootConf ConfWithA42()
      => new RootConf(new Dictionary<string, IConfDefinition> {
        {"A", defInt42}
      });

    private RootConf KeysWithExceptions()
      => new RootConf(new Dictionary<string, IConfDefinition> {
        {"A", defIntThrows},
        {"B", defIntCallsA},
      });
  }
}