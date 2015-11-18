using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Bud.Configuration {
  public class ScopedDictionaryBuilderTest {
    private ScopedDictionaryBuilder<string> builder;
    private IDictionary<string, string> dictionary;

    [SetUp]
    public void SetUp() {
      dictionary = new Dictionary<string, string>();
      builder = new ScopedDictionaryBuilder<string>(dictionary, ImmutableList<string>.Empty);
    }

    [Test]
    public void TryGetting_a_missing_key_must_return_false_and_output_null_value() {
      string confDefinition;
      Assert.False(builder.TryGetValue("A", out confDefinition));
      Assert.IsNull(confDefinition);
    }

    [Test]
    public void TryGetting_a_present_key_must_return_true_and_output_the_value() {
      string confDefinition;
      builder.Set("A", "foo");
      Assert.True(builder.TryGetValue("A", out confDefinition));
      Assert.AreEqual("foo", confDefinition);
    }

    [Test]
    public void Contains_must_return_false_for_undefined_keys()
      => Assert.False(builder.Contains("A"));

    [Test]
    public void Contains_must_return_true_for_defined_keys()
      => Assert.True(builder.Set("A", "foo").Contains("A"));

    [Test]
    public void Absolute_keys_can_be_referenced_by_relative_paths()
      => Assert.AreSame("foo", builder.Set("/A", "foo").Get("A"));

    [Test]
    public void Reseting_absolute_keys_with_relative_paths()
      => Assert.AreSame("bar", builder.Set("/A", "foo").Set("A", "bar").Get("A"));

    [Test]
    public void Relative_keys_can_be_set_in_nested_scopes()
      => Assert.AreSame("foo",
                        builder.In("B").Set("A", "foo").Out().Get("B/A"));

    [Test]
    public void Absolute_keys_can_be_set_in_nested_scopes()
      => Assert.AreSame("foo",
                        builder.In("B").Set("/A", "foo").Out().Get("/A"));

    [Test]
    public void Adding_backtracking_keys_in_nested_scopes_puts_them_into_root()
      => Assert.AreSame("foo",
                        builder.In("B").Set("../A", "foo").Out().Get("/A"));

    [Test]
    public void Getting_keys_via_a_relative_path()
      => Assert.AreSame("foo", builder.In("a").In("b").Set("A", "foo").Get("A"));

    [Test]
    public void TryGetting_keys_via_a_relative_path() {
      string aValue;
      builder.In("a").In("b").Set("A", "foo").TryGetValue("A", out aValue);
      Assert.AreSame("foo", aValue);
    }

    [Test]
    public void Adding_backtracking_keys_in_double_nested_scopes_puts_them_in_a_scope_above()
      => Assert.AreSame("foo",
                        builder.In("B").In("C").Set("../A", "foo").GoToRoot().Get("B/A"));

    [Test]
    public void Complex_setup() {
      builder.Set("a1", "valA1").Set("/r1", "valR1")
        .In("B").Set("b1", "valB1").Set("../a2", "valA2").Set("/r2", "valR2")
        .In("C").Set("c1", "valC1").Set("../b2", "valB2").Set("../../a3", "valA3").Set("/r3", "valR3")
        .In("D").Set("d1", "valD1").Set("../c2", "valC2").Set("../../b3", "valB3").Set("../../../a4", "valA4").Set("/r4", "valR4")
        .GoToRoot()
        .In("A").In(new[] {"B", "C", "D", "E"}).Set("e1", "valE1");
      var expectedDictionary = new Dictionary<string, string> {
        {"r1", "valR1"},
        {"r2", "valR2"},
        {"r3", "valR3"},
        {"r4", "valR4"},
        {"a1", "valA1"},
        {"a2", "valA2"},
        {"a3", "valA3"},
        {"a4", "valA4"},
        {"B/b1", "valB1"},
        {"B/b2", "valB2"},
        {"B/b3", "valB3"},
        {"B/C/c1", "valC1"},
        {"B/C/c2", "valC2"},
        {"B/C/D/d1", "valD1"},
        {"A/B/C/D/E/e1", "valE1"},
      };
      Assert.That(dictionary, Is.EquivalentTo(expectedDictionary));
    }
  }
}