using System.Collections.Generic;
using System.Collections.Immutable;
using NUnit.Framework;
using static NUnit.Framework.Assert;
using static Bud.Option;

namespace Bud.Configuration {
  public class ConfDictionaryTest {
    private ConfDirectory builder;
    private IDictionary<string, IConfDefinition> dictionary;
    private IConfDefinition fooConf;

    [SetUp]
    public void SetUp() {
      dictionary = new Dictionary<string, IConfDefinition>();
      builder = new ConfDirectory(dictionary, ImmutableList<string>.Empty);
      fooConf = NewConfDef("Foo");
    }

    [Test]
    public void TryGetting_a_missing_key_must_return_false_and_output_null_value()
      => AreEqual(None<IConfDefinition>(),
                  builder.TryGet("A"));

    [Test]
    public void TryGetting_a_present_key_must_return_true_and_output_the_value()
      => AreEqual(Some(fooConf),
                  builder.Set("A", fooConf).TryGet("A"));

    [Test]
    public void Contains_must_return_false_for_undefined_keys()
      => IsFalse(builder.TryGet("A").HasValue);

    [Test]
    public void Contains_must_return_true_for_defined_keys()
      => IsTrue(builder.Set("A", fooConf).TryGet("A").HasValue);

    [Test]
    public void Absolute_keys_can_be_referenced_by_relative_paths()
      => AreEqual(fooConf,
                  builder.Set("/A", fooConf).TryGet("A").Value);

    [Test]
    public void Reseting_absolute_keys_with_relative_paths() {
      var bar = NewConfDef("Bar");
      AreEqual(bar,
               builder.Set("/A", fooConf).Set("A", bar).TryGet("A").Value);
    }

    [Test]
    public void Relative_keys_can_be_set_in_nested_scopes() {
      builder.In("B").Set("A", fooConf);
      AreEqual(fooConf,
               builder.TryGet("B/A").Value);
    }

    [Test]
    public void Absolute_keys_can_be_set_in_nested_scopes() {
      builder.In("B").Set("/A", fooConf);
      AreEqual(fooConf,
               builder.TryGet("/A").Value);
    }

    [Test]
    public void Adding_backtracking_keys_in_nested_scopes_puts_them_into_root() {
      builder.In("B").Set("../A", fooConf);
      AreEqual(fooConf,
               builder.TryGet("/A").Value);
    }

    [Test]
    public void Getting_keys_via_a_relative_path()
      => AreEqual(fooConf,
                  builder.In("a").In("b").Set("A", fooConf).TryGet("A").Value);

    [Test]
    public void Adding_backtracking_keys_in_double_nested_scopes_puts_them_in_a_scope_above() {
      builder.In("B").In("C").Set("../A", fooConf);
      AreEqual(fooConf,
               builder.TryGet("B/A").Value);
    }

    [Test]
    public void Complex_setup() {
      var a1 = NewConfDef("valA1");
      var r1 = NewConfDef("valR1");
      var b1 = NewConfDef("valB1");
      var a2 = NewConfDef("valA2");
      var r2 = NewConfDef("valR2");
      var c1 = NewConfDef("valC1");
      var b2 = NewConfDef("valB2");
      var a3 = NewConfDef("valA3");
      var r3 = NewConfDef("valR3");
      var d1 = NewConfDef("valD1");
      var c2 = NewConfDef("valC2");
      var b3 = NewConfDef("valB3");
      var a4 = NewConfDef("valA4");
      var r4 = NewConfDef("valR4");
      var e1 = NewConfDef("valE1");
      builder.Set("a1", a1).Set("/r1", r1)
             .In("B").Set("b1", b1).Set("../a2", a2).Set("/r2", r2)
             .In("C").Set("c1", c1).Set("../b2", b2).Set("../../a3", a3).Set("/r3", r3)
             .In("D").Set("d1", d1).Set("../c2", c2).Set("../../b3", b3).Set("../../../a4", a4).Set("/r4", r4);
      builder.In("A").In(new[] {"B", "C", "D", "E"}).Set("e1", e1);
      var expectedDictionary = new Dictionary<string, IConfDefinition> {
        {"r1", r1},
        {"r2", r2},
        {"r3", r3},
        {"r4", r4},
        {"a1", a1},
        {"a2", a2},
        {"a3", a3},
        {"a4", a4},
        {"B/b1", b1},
        {"B/b2", b2},
        {"B/b3", b3},
        {"B/C/c1", c1},
        {"B/C/c2", c2},
        {"B/C/D/d1", d1},
        {"A/B/C/D/E/e1", e1},
      };
      That(dictionary, Is.EquivalentTo(expectedDictionary));
    }

    private static IConfDefinition NewConfDef(string confKey)
      => new ConfDefinition<string>(c => confKey);
  }
}