using System;
using NUnit.Framework;

namespace Bud {
  public class ScopeTest {

    private Key scopeA = new Key("A");
    private ConfigKey<string> configKeyA = new ConfigKey<string>("A");
    private TaskKey<int> taskKeyA = new TaskKey<int>("A");
    private Key scopeB = new Key("B");
    private ConfigKey<string> configKeyB = new ConfigKey<string>("B");
    private TaskKey<int> taskKeyB = new TaskKey<int>("B");
    private Key scopedScopeA = new Key("A").In(new Key("C"));
    private ConfigKey<string> scopedConfigKeyA = new ConfigKey<string>("A").In(new Key("C"));
    private TaskKey<int> scopedTaskKeyA = new TaskKey<int>("A").In(new Key("C"));

    [Test]
    public void Equals_MUST_return_true_WHEN_the_keys_have_the_same_id_and_same_scope() {
      Assert.AreEqual(scopeA, configKeyA);
      Assert.AreEqual(scopeA, taskKeyA);
      Assert.AreEqual(configKeyA, taskKeyA);
      Assert.AreEqual(configKeyA, scopeA);
      Assert.AreEqual(taskKeyA, scopeA);
      Assert.AreEqual(taskKeyA, configKeyA);
      Assert.AreEqual(scopeA, scopeA);
      Assert.AreEqual(configKeyA, configKeyA);
      Assert.AreEqual(taskKeyA, taskKeyA);
    }

    [Test]
    public void GetHashCode_MUST_return_the_same_values_FOR_keys_with_the_same_id_and_same_scope() {
      Assert.AreEqual(scopeA.GetHashCode(), configKeyA.GetHashCode());
      Assert.AreEqual(scopeA.GetHashCode(), taskKeyA.GetHashCode());
    }

    [Test]
    public void Equals_MUST_return_false_WHEN_the_keys_have_a_different_id() {
      Assert.AreNotEqual(scopeA, configKeyB);
      Assert.AreNotEqual(scopeA, taskKeyB);
      Assert.AreNotEqual(configKeyA, taskKeyB);
      Assert.AreNotEqual(configKeyB, scopeA);
      Assert.AreNotEqual(taskKeyB, scopeA);
      Assert.AreNotEqual(taskKeyB, configKeyA);
      Assert.AreNotEqual(scopeA, scopeB);
      Assert.AreNotEqual(configKeyA, configKeyB);
      Assert.AreNotEqual(taskKeyA, taskKeyB);
      Assert.AreNotEqual(scopeB, scopeA);
      Assert.AreNotEqual(configKeyB, configKeyA);
      Assert.AreNotEqual(taskKeyB, taskKeyA);
      Assert.AreNotEqual(configKeyA, scopeB);
      Assert.AreNotEqual(taskKeyA, scopeB);
      Assert.AreNotEqual(scopeB, configKeyA);
      Assert.AreNotEqual(scopeB, taskKeyA);
    }

    [Test]
    public void Equals_MUST_return_different_has_codes_WHEN_the_keys_have_a_different_ids() {
      Assert.AreNotEqual(scopeA.GetHashCode(), scopeB.GetHashCode());
      Assert.AreNotEqual(configKeyA.GetHashCode(), configKeyB.GetHashCode());
      Assert.AreNotEqual(taskKeyA.GetHashCode(), taskKeyB.GetHashCode());
    }

    [Test]
    public void Equals_MUST_return_false_WHEN_the_keys_have_a_different_scope() {
      Assert.AreNotEqual(scopeA, scopedConfigKeyA);
      Assert.AreNotEqual(scopeA, scopedTaskKeyA);
      Assert.AreNotEqual(configKeyA, scopedTaskKeyA);
      Assert.AreNotEqual(scopedConfigKeyA, scopeA);
      Assert.AreNotEqual(scopedTaskKeyA, scopeA);
      Assert.AreNotEqual(scopedTaskKeyA, configKeyA);
      Assert.AreNotEqual(scopeA, scopedScopeA);
      Assert.AreNotEqual(configKeyA, scopedConfigKeyA);
      Assert.AreNotEqual(taskKeyA, scopedTaskKeyA);
      Assert.AreNotEqual(scopedScopeA, scopeA);
      Assert.AreNotEqual(scopedConfigKeyA, configKeyA);
      Assert.AreNotEqual(scopedTaskKeyA, taskKeyA);
      Assert.AreNotEqual(configKeyA, scopedScopeA);
      Assert.AreNotEqual(taskKeyA, scopedScopeA);
      Assert.AreNotEqual(scopedScopeA, configKeyA);
      Assert.AreNotEqual(scopedScopeA, taskKeyA);
    }

    [Test]
    public void GetHashCode_MUST_be_different_WHEN_the_scopes_are_not_equal() {
      Assert.AreNotEqual(scopeA.GetHashCode(), scopedScopeA.GetHashCode());
      Assert.AreNotEqual(configKeyA.GetHashCode(), scopedConfigKeyA.GetHashCode());
      Assert.AreNotEqual(taskKeyA.GetHashCode(), scopedTaskKeyA.GetHashCode());
    }

    [Test]
    public void ToString_MUST_return_the_id_of_the_key_WHEN_its_scope_is_global() {
      Assert.AreEqual(":A", scopeA.ToString());
    }

    [Test]
    public void ToString_MUST_return_the_id_and_the_ids_of_scopes_WHEN_the_key_is_nested_in_multiple_scopes() {
      var deeplyNestedKey = taskKeyA
        .In(new TaskKey<uint>("E").In(new Key("D").In(new Key("foo").In(new ConfigKey<bool>("C").In(configKeyB)))));
      Assert.AreEqual(":B:C:foo:D:E:A", deeplyNestedKey.ToString());
    }

    [Test]
    public void Equals_MUST_return_true_WHEN_two_key_instances_have_the_same_id_and_scope() {
      var scopedConfigKeyAClone = new ConfigKey<string>("A").In(new Key("C"));
      Assert.AreEqual(scopedConfigKeyA.GetHashCode(), scopedConfigKeyAClone.GetHashCode());
      Assert.AreEqual(scopedConfigKeyA, scopedConfigKeyAClone);
    }

    [Test]
    public void Concat_MUST_not_change_the_scope_WHEN_the_parent_is_global() {
      Assert.AreEqual(scopeA, Key.Concat(Key.Global, scopeA));
    }

    [Test]
    public void Concat_MUST_not_change_the_scope_WHEN_the_child_is_global() {
      Assert.AreEqual(scopeB, Key.Concat(scopeB, Key.Global));
    }

    [Test]
    public void Concat_MUST_add_the_single_parent_to_the_child_with_no_parents() {
      Assert.AreEqual(new Key("A", scopeB), Key.Concat(scopeB, scopeA));
    }

    [Test]
    public void Concat_MUST_add_the_parent_hierarchy_to_the_child_with_no_parents() {
      var scopedScope = new Key("B", new Key("C"));
      Assert.AreEqual(new Key("A", scopedScope), Key.Concat(scopedScope, new Key("A")));
    }

    [Test]
    public void Concat_MUST_add_the_parent_to_the_child_with_parents() {
      Assert.AreEqual(
        new Key("A", new Key("B", new Key("C"))),
        Key.Concat(new Key("C"), new Key("A", new Key("B")))
      );
    }

    [Test]
    public void Concat_MUST_add_the_parents_to_the_child_with_parents() {
      Assert.AreEqual(
        new Key("A", new Key("B", new Key("C", new Key("D")))),
        Key.Concat(new Key("C", new Key("D")), new Key("A", new Key("B")))
      );
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Parse_MUST_throw_an_exception_WHEN_given_an_empty_string() {
      Key.Parse(String.Empty);
    }

    [Test]
    public void Parse_MUST_return_the_global_scope() {
      var parsedScope = Key.Parse(":");
      Assert.AreEqual(Key.Global, parsedScope);
    }

    [Test]
    public void Parse_MUST_a_child_scope_of_the_global_scope() {
      var parsedScope = Key.Parse("child");
      Assert.AreEqual(new Key("child"), parsedScope);
    }

    [Test]
    public void Parse_MUST_a_child_scope_of_the_global_scope_WHEN_prefixed_with_the_global_colon() {
      var parsedScope = Key.Parse(":child");
      Assert.AreEqual(new Key("child"), parsedScope);
    }

    [Test]
    public void Parse_MUST_return_a_chain_of_scopes() {
      var parsedScope = Key.Parse("parent:child");
      Assert.AreEqual(new Key("child").In(new Key("parent")), parsedScope);
    }

    [Test]
    public void Parse_MUST_return_a_chain_of_scopes_WHEN_prefixed_with_the_global_colon() {
      var parsedScope = Key.Parse(":parent:child");
      Assert.AreEqual(new Key("child").In(new Key("parent")), parsedScope);
    }

    [Test]
    public void Parse_MUST_perform_the_inverse_of_ToString() {
      var deeplyNestedKey = taskKeyA.In(new TaskKey<uint>("E").In(new Key("D").In(new Key("foo").In(new ConfigKey<bool>("C").In(configKeyB)))));
      Assert.AreEqual(deeplyNestedKey, Key.Parse(deeplyNestedKey.ToString()));
      Assert.AreEqual(":B:C:foo:D:E:A", Key.Parse(":B:C:foo:D:E:A").ToString());
    }
  }
}

