using System;
using NUnit.Framework;

namespace Bud {
  public class KeyPathUtilsTest {
    [Test]
    public void IsAbsolutePath_MUST_return_false_WHEN_given_a_relative_path() {
      Assert.IsFalse(KeyPathUtils.IsAbsolutePath("foo"));
    }

    [Test]
    public void IsAbsolutePath_MUST_return_true_WHEN_given_an_absolute_path() {
      Assert.IsTrue(KeyPathUtils.IsAbsolutePath("/foo"));
      Assert.IsTrue(KeyPathUtils.IsAbsolutePath("/"));
    }

    [Test]
    public void IsRootPath_MUST_return_false_WHEN_given_a_relative_path() {
      Assert.IsFalse(KeyPathUtils.IsRootPath("a/b"));
    }

    [Test]
    public void IsRootPath_MUST_return_false_WHEN_given_a_non_empty_absolute_path() {
      Assert.IsFalse(KeyPathUtils.IsRootPath("/a/b"));
    }

    [Test]
    public void IsRootPath_MUST_return_true_WHEN_given_the_root_path() {
      Assert.IsTrue(KeyPathUtils.IsRootPath("/"));
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void ExtractIdFromPath_MUST_throw_an_exception_WHEN_given_an_empty_string() {
      KeyPathUtils.ExtractIdFromPath("");
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void ExtractIdFromPath_MUST_throw_an_exception_WHEN_the_last_component_of_a_relative_composite_path_is_empty() {
      KeyPathUtils.ExtractIdFromPath("a/");
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void ExtractIdFromPath_MUST_throw_an_exception_WHEN_the_last_component_of_an_absolute_composite_path_is_empty() {
      KeyPathUtils.ExtractIdFromPath("/a/");
    }

    [Test]
    public void ExtractIdFromPath_MUST_return_root_WHEN_given_the_root_path() {
      Assert.AreEqual(Key.RootId, KeyPathUtils.ExtractIdFromPath("/"));
    }

    [Test]
    public void ExtractIdFromPath_MUST_return_the_last_component_WHEN_given_a_composite_path() {
      Assert.AreEqual("foo", KeyPathUtils.ExtractIdFromPath("bar/a/foo"));
      Assert.AreEqual("foo", KeyPathUtils.ExtractIdFromPath("/bar/a/foo"));
    }

    [Test]
    public void ExtractIdFromPath_MUST_return_the_whole_path_WHEN_a_single_component_path() {
      Assert.AreEqual("bar", KeyPathUtils.ExtractIdFromPath("bar"));
      Assert.AreEqual("bar", KeyPathUtils.ExtractIdFromPath("/bar"));
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void ExtractParentPath_MUST_throw_an_exception_WHEN_given_a_single_component_relative_path() {
      KeyPathUtils.ExtractParentPath("bar");
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void ExtractParentPath_MUST_throw_an_exception_WHEN_given_the_root_path() {
      KeyPathUtils.ExtractParentPath("/");
    }

    [Test]
    public void ExtractParentPath_MUST_return_the_root_id_WHEN_given_a_single_component_absolute_path() {
      Assert.AreSame(Key.KeySeparatorAsString, KeyPathUtils.ExtractParentPath("/bar"));
    }

    [Test]
    public void ExtractParentPath_MUST_return_the_path_before_the_last_separator_WHEN_given_a_composite_path() {
      Assert.AreEqual("foo/bar", KeyPathUtils.ExtractParentPath("foo/bar/zoo"));
      Assert.AreEqual("/foo/bar", KeyPathUtils.ExtractParentPath("/foo/bar/zoo"));
    }

    [Test]
    public void JoinPath_MUST_prepent_the_relative_path_with_a_slash_WHEN_prepending_the_root_to_the_relative_path() {
      Assert.AreEqual("/foo", KeyPathUtils.JoinPath(Key.Root.Path, "foo"));
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void ParseId_MUST_throw_an_exception_WHEN_the_given_string_contains_a_key_separator() {
      KeyPathUtils.ParseId("a/b");
    }

    [Test]
    public void ParseId_MUST_return_the_same_string_WHEN_it_does_not_contain_a_key_separator() {
      const string id = "foo";
      Assert.AreSame(id, KeyPathUtils.ParseId(id));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void NormalizePath_MUST_throw_an_exception_WHEN_given_an_empty_string() {
      KeyPathUtils.NormalizePath(string.Empty);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void NormalizePath_MUST_throw_an_exception_WHEN_given_null() {
      KeyPathUtils.NormalizePath(null);
    }

    [Test]
    public void NormalizePath_MUST_return_the_given_already_normalized_path() {
      const string somePath = "/some/path";
      Assert.AreSame(somePath, KeyPathUtils.NormalizePath(somePath));
    }
  }
}