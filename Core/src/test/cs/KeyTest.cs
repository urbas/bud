using System;
using NUnit.Framework;

namespace Bud {
  public class KeyTest {

    private Key keyA = new Key("A");
    private ConfigKey<string> configKeyA = new ConfigKey<string>("A");
    private TaskKey<int> taskKeyA = new TaskKey<int>("A");
    private Key keyB = new Key("B");
    private ConfigKey<string> configKeyB = new ConfigKey<string>("B");
    private TaskKey<int> taskKeyB = new TaskKey<int>("B");
    private Key keydKeyA = new Key("A").In(new Key("C"));
    private ConfigKey<string> keydConfigKeyA = new ConfigKey<string>("A").In(new Key("C"));
    private TaskKey<int> keydTaskKeyA = new TaskKey<int>("A").In(new Key("C"));

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
      var deeplyNestedKey = taskKeyA
        .In(new TaskKey<uint>("E").In(new Key("D").In(new Key("foo").In(new ConfigKey<bool>("C").In(configKeyB)))));
      Assert.AreEqual("B/C/foo/D/E/A", deeplyNestedKey.ToString());
    }

    [Test]
    public void Equals_MUST_return_true_WHEN_two_key_instances_have_the_same_id_and_key() {
      var keydConfigKeyAClone = new ConfigKey<string>("A").In(new Key("C"));
      Assert.AreEqual(keydConfigKeyA.GetHashCode(), keydConfigKeyAClone.GetHashCode());
      Assert.AreEqual(keydConfigKeyA, keydConfigKeyAClone);
    }

    [Test]
    public void Concat_MUST_parent_the_key_to_root_WHEN_the_key_is_relative() {
      Assert.AreEqual(keyB.In(Key.Root), Key.Concat(Key.Root, keyB));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Concat_MUST_throw_an_exception_WHEN_trying_to_add_a_parent_to_roor() {
      Key.Concat(keyB, Key.Root);
    }

    [Test]
    public void Concat_MUST_add_the_single_parent_to_the_child_with_no_parents() {
      Assert.AreEqual(new Key("A", keyB), Key.Concat(keyB, keyA));
    }

    [Test]
    public void Concat_MUST_add_the_parent_hierarchy_to_the_child_with_no_parents() {
      var keydKey = new Key("B", new Key("C"));
      Assert.AreEqual(new Key("A", keydKey), Key.Concat(keydKey, new Key("A")));
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
    public void Parse_MUST_return_the_global_key() {
      var parsedKey = Key.Parse("/");
      Assert.AreEqual(Key.Root, parsedKey);
    }

    [Test]
    public void Parse_MUST_return_a_key() {
      var parsedKey = Key.Parse("child");
      Assert.AreEqual(new Key("child"), parsedKey);
    }

    [Test]
    public void Parse_MUST_a_child_key_of_the_global_key_WHEN_prefixed_with_the_global_colon() {
      var parsedKey = Key.Parse("/child");
      Assert.AreEqual(new Key("child", Key.Root), parsedKey);
    }

    [Test]
    public void Parse_MUST_return_a_chain_of_keys() {
      var parsedKey = Key.Parse("parent/child");
      Assert.AreEqual(new Key("child").In(new Key("parent")), parsedKey);
    }

    [Test]
    public void Parse_MUST_return_a_chain_of_keys_WHEN_prefixed_with_the_global_colon() {
      var parsedKey = Key.Parse("/parent/child");
      Assert.AreEqual(new Key("child").In(new Key("parent")).In(Key.Root), parsedKey);
    }

    [Test]
    public void Parse_MUST_perform_the_inverse_of_ToString() {
      var deeplyNestedKey = taskKeyA.In(new TaskKey<uint>("E").In(new Key("D").In(new Key("foo").In(new ConfigKey<bool>("C").In(configKeyB)))));
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
      Assert.IsTrue(taskKeyA.In(Key.Root).IsAbsolute);
    }
  }
}

