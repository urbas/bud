using System;
using NUnit.Framework;

namespace Bud {
  public class KeyTest {
    private Key keyA = Key.Define("A");
    private ConfigKey<string> configKeyA = Key.Define("A");
    private TaskKey<int> taskKeyA = Key.Define("A");
    private Key keyB = Key.Define("B");
    private ConfigKey<string> configKeyB = Key.Define("B");
    private TaskKey<int> taskKeyB = Key.Define("B");
    private Key keydKeyA = Key.Define("C") / Key.Define("A");
    private ConfigKey<string> keydConfigKeyA = Key.Define("C") / "A";
    private TaskKey<int> keydTaskKeyA = Key.Define("C") / "A";

    [Test]
    public void Equals_MUST_return_true_WHEN_the_keys_have_the_same_id_and_same_key() {
      Assert.AreEqual(keyA, configKeyA);
      Assert.AreEqual(keyA, taskKeyA);
      Assert.AreEqual(configKeyA, taskKeyA);
      Assert.AreEqual(configKeyA, keyA);
      Assert.AreEqual(taskKeyA, keyA);
      Assert.AreEqual(taskKeyA, configKeyA);
      Assert.AreEqual(keyA, keyA);
      Assert.AreEqual(configKeyA, configKeyA);
      Assert.AreEqual(taskKeyA, taskKeyA);
    }

    [Test]
    public void GetHashCode_MUST_return_the_same_values_FOR_keys_with_the_same_id_and_same_key() {
      Assert.AreEqual(keyA.GetHashCode(), configKeyA.GetHashCode());
      Assert.AreEqual(keyA.GetHashCode(), taskKeyA.GetHashCode());
    }

    [Test]
    public void Equals_MUST_return_false_WHEN_the_keys_have_a_different_id() {
      Assert.AreNotEqual(keyA, configKeyB);
      Assert.AreNotEqual(keyA, taskKeyB);
      Assert.AreNotEqual(configKeyA, taskKeyB);
      Assert.AreNotEqual(configKeyB, keyA);
      Assert.AreNotEqual(taskKeyB, keyA);
      Assert.AreNotEqual(taskKeyB, configKeyA);
      Assert.AreNotEqual(keyA, keyB);
      Assert.AreNotEqual(configKeyA, configKeyB);
      Assert.AreNotEqual(taskKeyA, taskKeyB);
      Assert.AreNotEqual(keyB, keyA);
      Assert.AreNotEqual(configKeyB, configKeyA);
      Assert.AreNotEqual(taskKeyB, taskKeyA);
      Assert.AreNotEqual(configKeyA, keyB);
      Assert.AreNotEqual(taskKeyA, keyB);
      Assert.AreNotEqual(keyB, configKeyA);
      Assert.AreNotEqual(keyB, taskKeyA);
    }

    [Test]
    public void Equals_MUST_return_different_has_codes_WHEN_the_keys_have_a_different_ids() {
      Assert.AreNotEqual(keyA.GetHashCode(), keyB.GetHashCode());
      Assert.AreNotEqual(configKeyA.GetHashCode(), configKeyB.GetHashCode());
      Assert.AreNotEqual(taskKeyA.GetHashCode(), taskKeyB.GetHashCode());
    }

    [Test]
    public void Equals_MUST_return_false_WHEN_the_keys_have_a_different_key() {
      Assert.AreNotEqual(keyA, keydConfigKeyA);
      Assert.AreNotEqual(keyA, keydTaskKeyA);
      Assert.AreNotEqual(configKeyA, keydTaskKeyA);
      Assert.AreNotEqual(keydConfigKeyA, keyA);
      Assert.AreNotEqual(keydTaskKeyA, keyA);
      Assert.AreNotEqual(keydTaskKeyA, configKeyA);
      Assert.AreNotEqual(keyA, keydKeyA);
      Assert.AreNotEqual(configKeyA, keydConfigKeyA);
      Assert.AreNotEqual(taskKeyA, keydTaskKeyA);
      Assert.AreNotEqual(keydKeyA, keyA);
      Assert.AreNotEqual(keydConfigKeyA, configKeyA);
      Assert.AreNotEqual(keydTaskKeyA, taskKeyA);
      Assert.AreNotEqual(configKeyA, keydKeyA);
      Assert.AreNotEqual(taskKeyA, keydKeyA);
      Assert.AreNotEqual(keydKeyA, configKeyA);
      Assert.AreNotEqual(keydKeyA, taskKeyA);
    }

    [Test]
    public void GetHashCode_MUST_be_different_WHEN_the_keys_are_not_equal() {
      Assert.AreNotEqual(keyA.GetHashCode(), keydKeyA.GetHashCode());
      Assert.AreNotEqual(configKeyA.GetHashCode(), keydConfigKeyA.GetHashCode());
      Assert.AreNotEqual(taskKeyA.GetHashCode(), keydTaskKeyA.GetHashCode());
    }

    [Test]
    public void ToString_MUST_return_the_id_of_the_key_WHEN_its_key_is_global() {
      Assert.AreEqual("A", keyA.ToString());
    }

    [Test]
    public void ToString_MUST_return_the_id_and_the_ids_of_keys_WHEN_the_key_is_nested_in_multiple_keys() {
      var deeplyNestedKey = configKeyB / "C" / Key.Define("foo") / "D" / Key.Define("E") / taskKeyA;
      Assert.AreEqual("B/C/foo/D/E/A", deeplyNestedKey.ToString());
    }

    [Test]
    public void Equals_MUST_return_true_WHEN_two_key_instances_have_the_same_id_and_key() {
      ConfigKey<string> keydConfigKeyAClone = Key.Define("C") / "A";
      Assert.AreEqual(keydConfigKeyA.GetHashCode(), keydConfigKeyAClone.GetHashCode());
      Assert.AreEqual(keydConfigKeyA, keydConfigKeyAClone);
    }

    [Test]
    public void Concat_MUST_parent_the_key_to_root_WHEN_the_key_is_relative() {
      Assert.AreEqual(Key.Parse("/B"), Key.Root / keyB);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Concat_MUST_throw_an_exception_WHEN_trying_to_add_a_parent_to_root() {
      var _ = keyB / Key.Root;
    }

    [Test]
    public void Concat_MUST_return_the_same_absolute_path_WHEN_trying_to_parent_it_to_root() {
      var absPath = Key.Parse("/foo/bar");
      Assert.AreSame(absPath, Key.Root / absPath);
    }

    [Test]
    public void Concat_MUST_return_a_key_with_concatenated_paths() {
      var parent = Key.Parse("/foo/bar/zar");
      var child = Key.Parse("a/b/c");
      Assert.AreEqual(Key.Parse("/foo/bar/zar/a/b/c"), parent / child);
      Assert.AreEqual("/foo/bar/zar/a/b/c", (parent / child).ToString());
    }

    [Test]
    public void Concat_MUST_add_the_single_parent_to_the_child_with_no_parents() {
      Assert.AreEqual(Key.Parse("A/B"), keyA / keyB);
    }

    [Test]
    public void Concat_MUST_add_the_parent_hierarchy_to_the_child_with_no_parents() {
      var keydKey = Key.Parse("C/B");
      Assert.AreEqual(Key.Parse("C/B/A"), keydKey / Key.Define("A"));
    }

    [Test]
    public void Concat_MUST_add_the_parent_to_the_child_with_parents() {
      Assert.AreEqual(
        Key.Define(Key.Define("A"), Key.Define(Key.Define("B"), Key.Define("C"))),
        Key.Define("A") / Key.Define("B") / Key.Define("C")
        );
    }

    [Test]
    public void Concat_MUST_add_the_parents_to_the_child_with_parents() {
      Assert.AreEqual(
        Key.Parse("A/B/C/D"),
        Key.Define(Key.Define(Key.Define("A"), Key.Define("B")), Key.Define(Key.Define("C"), Key.Define("D")))
        );
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Parse_MUST_throw_an_exception_WHEN_given_an_empty_string() {
      Key.Parse(String.Empty);
    }

    [Test]
    public void Parse_MUST_return_the_global_key() {
      var parsedKey = Key.Parse("/");
      Assert.AreEqual(Key.Root, parsedKey);
    }

    [Test]
    public void Parse_MUST_return_a_key() {
      var parsedKey = Key.Parse("child");
      Assert.AreEqual(Key.Define("child"), parsedKey);
    }

    [Test]
    public void Parse_MUST_a_child_key_of_the_global_key_WHEN_prefixed_with_the_global_colon() {
      var parsedKey = Key.Parse("/child");
      Assert.AreEqual(Key.Root / Key.Define("child"), parsedKey);
    }

    [Test]
    public void Parse_MUST_return_a_chain_of_keys() {
      var parsedKey = Key.Parse("parent/child");
      Assert.AreEqual(Key.Define("parent") / Key.Define("child"), parsedKey);
    }

    [Test]
    public void Parse_MUST_return_a_chain_of_keys_WHEN_prefixed_with_the_global_colon() {
      var parsedKey = Key.Parse("/parent/child");
      Assert.AreEqual(Key.Root / Key.Define("parent") / Key.Define("child"), parsedKey);
    }

    [Test]
    public void Parse_MUST_return_the_singleton_root_instance_WHEN_given_the_root_path() {
      Assert.AreSame(Key.Root, Key.Parse("/"));
    }

    [Test]
    public void Parse_MUST_perform_the_inverse_of_ToString() {
      var deeplyNestedKey = configKeyB / Key.Define("C") / Key.Define("foo") / Key.Define("D") / Key.Define("E") / taskKeyA;
      Assert.AreEqual(deeplyNestedKey, Key.Parse(deeplyNestedKey.ToString()));
      Assert.AreEqual("B/C/foo/D/E/A", Key.Parse("B/C/foo/D/E/A").ToString());
      Assert.AreEqual("/B/C/foo/D/E/A", Key.Parse("/B/C/foo/D/E/A").ToString());
    }

    [Test]
    public void IsAbsolute_MUST_return_false_WHEN_creating_a_simple_key() {
      Assert.IsFalse(taskKeyA.IsAbsolute);
    }

    [Test]
    public void IsAbsolute_MUST_return_true_WHEN_parented_to_global() {
      Assert.IsTrue((Key.Root / taskKeyA).IsAbsolute);
    }

    [Test]
    public void Parent_MUST_return_the_path_without_the_leaf_WHEN_two_long_keys_are_concatenated() {
      var parent = Key.Parse("/foo/bar/zar");
      var child = Key.Parse("a/b/c");
      Assert.AreEqual(Key.Parse("/foo/bar/zar/a/b"), (parent / child).Parent);
    }

    [Test]
    public void Parent_MUST_return_the_root_WHEN_given_a_single_component_absolute_path() {
      Assert.AreSame(Key.Root, Key.Parse("/bar").Parent);
    }
  }
}