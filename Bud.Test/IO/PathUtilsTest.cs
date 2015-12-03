using System;
using NUnit.Framework;
using static System.IO.Path;

namespace Bud.IO {
  public class PathUtilsTest {
    [Test]
    public void IsPathInDir_throws_if_path_is_null()
      => Assert.Throws<ArgumentNullException>(() => PathUtils.IsPathInDir(null, string.Empty));

    [Test]
    public void IsPathInDir_throws_if_dir_is_null()
      => Assert.Throws<ArgumentNullException>(() => PathUtils.IsPathInDir(string.Empty, null));

    [Test]
    public void IsPathInDir_returns_true_if_path_and_dir_are_empty_strings()
      => Assert.IsTrue(PathUtils.IsPathInDir(string.Empty, string.Empty));

    [Test]
    public void IsPathInDir_returns_false_if_path_is_empty_and_dir_is_non_empty()
      => Assert.IsFalse(PathUtils.IsPathInDir(string.Empty, "a"));

    [Test]
    public void IsPathInDir_returns_true_if_path_and_dir_equal()
      => Assert.IsTrue(PathUtils.IsPathInDir("a/", "a"));

    [Test]
    public void IsPathInDir_returns_true_if_path_ends_with_dir_separator()
      => Assert.IsTrue(PathUtils.IsPathInDir("a/", "a"));

    [Test]
    public void IsPathInDir_returns_true_if_path_is_a_subpath_of_dir()
      => Assert.IsTrue(PathUtils.IsPathInDir("a/b", "a"));

    [Test]
    public void IsPathInDir_returns_false_if_path_is_a_substring_of_dir_but_not_a_subpath()
      => Assert.IsFalse(PathUtils.IsPathInDir("ab", "a"));

    [Test]
    public void IsPathInDir_returns_true_if_dir_ends_with_dir_separator()
      => Assert.IsTrue(PathUtils.IsPathInDir("a", "a/"));

    [Test]
    public void IsPathInDir_returns_false_for_diverging_paths()
      => Assert.IsFalse(PathUtils.IsPathInDir("ac", "ab"));

    [Test]
    public void IsPathInDir_returns_true_for_multi_level_paths()
      => Assert.IsFalse(PathUtils.IsPathInDir("a/b/c/d", "a/b/c/"));

    [Test]
    public void IsPathInDir_returns_true_for_multi_level_paths_with_platform_specific_path_separators() {
      Assert.IsTrue(PathUtils.IsPathInDir($"a{DirectorySeparatorChar}b", "a/b/c/"));
      Assert.IsTrue(PathUtils.IsPathInDir("a/b/c/", $"a{DirectorySeparatorChar}b"));
    }
  }

}