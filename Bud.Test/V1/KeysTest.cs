using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;

namespace Bud.V1 {
  public class KeysTest {
    private readonly ImmutableArray<string> abcDir = ImmutableArray.Create("a", "b", "c");

    [Test]
    public void CountBacktracks_must_return_0_for_absolute_keys()
      => Assert.AreEqual(0, Keys.CountBacktracks("/foo"));

    [Test]
    public void CountBacktracks_must_return_1_for_single_backtracked_keys()
      => Assert.AreEqual(1, Keys.CountBacktracks("../foo"));

    [Test]
    public void CountBacktracks_must_return_2_for_double_backtracked_keys()
      => Assert.AreEqual(2, Keys.CountBacktracks("../../foo"));

    [Test]
    public void DirToString_returns_an_empty_string_when_given_an_empty_dir()
      => Assert.IsEmpty(Keys.DirToString(Enumerable.Empty<string>()));

    [Test]
    public void DirToString_single_element_dir()
      => Assert.AreEqual("foo", Keys.DirToString(new[] {"foo"}));

    [Test]
    public void DirToString_multi_element_dir()
      => Assert.AreEqual("foo/bar/zar",
                         Keys.DirToString(new[] {"foo", "bar", "zar"}));

    [Test]
    public void ToFullPath_returns_the_same_string_when_dir_is_empty()
      => Assert.AreEqual("foo",
                         Keys.ToFullPath("foo", ImmutableList<string>.Empty));

    [Test]
    public void ToFullPath_removes_the_leading_separator()
      => Assert.AreEqual("foo",
                         Keys.ToFullPath("/foo", ImmutableList<string>.Empty));

    [Test]
    public void ToFullPath_removes_the_leading_separator_for_any_dir()
      => Assert.AreEqual("foo",
                         Keys.ToFullPath("/foo", abcDir));

    [Test]
    public void ToFullPath_prepends_the_dir()
      => Assert.AreEqual("a/b/c/foo",
                         Keys.ToFullPath("foo", abcDir));

    [Test]
    public void ToFullPath_prepends_partial_dirs_when_backtracks_are_present()
      => Assert.AreEqual("a/foo",
                         Keys.ToFullPath("../../foo", abcDir));

    [Test]
    public void InterpretFromDir_returns_unchanged_absolute_keys()
      => Assert.AreEqual("/A", Keys.InterpretFromDir("/A", abcDir));

    [Test]
    public void InterpretFromDir_prepends_dir_to_relative_paths()
      => Assert.AreEqual("a/b/c/A", Keys.InterpretFromDir("A", abcDir));

    [Test]
    public void InterpretFromDir_returns_unchanged_relative_keys_when_dir_is_empty()
      => Assert.AreEqual("A", Keys.InterpretFromDir("A", ImmutableList<string>.Empty));

    [Test]
    public void InterpretFromDir_removes_all_backtracks_when_dir_size_matches_the_number_of_backtracks()
      => Assert.AreEqual("A", Keys.InterpretFromDir("../../../A", abcDir));

    [Test]
    public void InterpretFromDir_removes_some_backtracks_when_they_outnumber_the_dir_depth()
      => Assert.AreEqual("../A", Keys.InterpretFromDir("../../../../A", abcDir));

    [Test]
    public void InterpretFromDir_prepends_some_of_the_dir_when_dir_depth_outnumbers_backtracks()
      => Assert.AreEqual("a/b/A", Keys.InterpretFromDir("../A", abcDir));
  }
}