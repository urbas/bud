using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;

namespace Bud {
  public class KeysTest {
    private readonly ImmutableArray<string> abcScope = ImmutableArray.Create("a", "b", "c");

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
    public void ConvertScopeToString_returns_an_empty_string_when_given_an_empty_scope()
      => Assert.IsEmpty(Keys.ConvertScopeToString(Enumerable.Empty<string>()));

    [Test]
    public void ConvertScopeToString_single_element_scope()
      => Assert.AreEqual("foo", Keys.ConvertScopeToString(new[] {"foo"}));

    [Test]
    public void ConvertScopeToString_multi_element_scope()
      => Assert.AreEqual("foo/bar/zar",
                         Keys.ConvertScopeToString(new[] {"foo", "bar", "zar"}));

    [Test]
    public void ToFullPath_returns_the_same_string_when_scope_is_empty()
      => Assert.AreEqual("foo",
                         Keys.ToFullPath("foo", ImmutableList<string>.Empty));

    [Test]
    public void ToFullPath_removes_the_leading_separator()
      => Assert.AreEqual("foo",
                         Keys.ToFullPath("/foo", ImmutableList<string>.Empty));

    [Test]
    public void ToFullPath_removes_the_leading_separator_for_any_scope()
      => Assert.AreEqual("foo",
                         Keys.ToFullPath("/foo", abcScope));

    [Test]
    public void ToFullPath_prepends_the_scope()
      => Assert.AreEqual("a/b/c/foo",
                         Keys.ToFullPath("foo", abcScope));

    [Test]
    public void ToFullPath_prepends_partial_scopes_when_backtracks_are_present()
      => Assert.AreEqual("a/foo",
                         Keys.ToFullPath("../../foo", abcScope));

    [Test]
    public void InterpretFromScope_returns_unchanged_absolute_keys()
      => Assert.AreEqual("/A", Keys.InterpretFromScope("/A", abcScope));

    [Test]
    public void InterpretFromScope_prepends_scope_to_relative_paths()
      => Assert.AreEqual("a/b/c/A", Keys.InterpretFromScope("A", abcScope));

    [Test]
    public void InterpretFromScope_returns_unchanged_relative_keys_when_scope_is_empty()
      => Assert.AreEqual("A", Keys.InterpretFromScope("A", ImmutableList<string>.Empty));

    [Test]
    public void InterpretFromScope_removes_all_backtracks_when_scope_size_matches_the_number_of_backtracks()
      => Assert.AreEqual("A", Keys.InterpretFromScope("../../../A", abcScope));

    [Test]
    public void InterpretFromScope_removes_some_backtracks_when_they_outnumber_the_scope_depth()
      => Assert.AreEqual("../A", Keys.InterpretFromScope("../../../../A", abcScope));

    [Test]
    public void InterpretFromScope_prepends_some_of_the_scope_when_scope_depth_outnumbers_backtracks()
      => Assert.AreEqual("a/b/A", Keys.InterpretFromScope("../A", abcScope));
  }
}