using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Bud.V1 {
  public class KeysTest {
    private readonly ImmutableArray<string> abcScope = ImmutableArray.Create("a", "b", "c");

    [Test]
    public void CountBacktracks_must_return_0_for_absolute_keys()
      => AreEqual(0, Keys.CountBacktracks("/foo"));

    [Test]
    public void CountBacktracks_must_return_1_for_single_backtracked_keys()
      => AreEqual(1, Keys.CountBacktracks("../foo"));

    [Test]
    public void CountBacktracks_must_return_2_for_double_backtracked_keys()
      => AreEqual(2, Keys.CountBacktracks("../../foo"));

    [Test]
    public void ConvertScopeToString_returns_an_empty_string_when_given_an_empty_scope()
      => IsEmpty(Keys.ConvertScopeToString(Enumerable.Empty<string>()));

    [Test]
    public void ConvertScopeToString_single_element_scope()
      => AreEqual("foo", Keys.ConvertScopeToString(new[] {"foo"}));

    [Test]
    public void ConvertScopeToString_multi_element_scope()
      => AreEqual("foo/bar/zar",
                  Keys.ConvertScopeToString(new[] {"foo", "bar", "zar"}));

    [Test]
    public void ToFullPath_returns_the_same_string_when_scope_is_empty()
      => AreEqual("foo",
                  Keys.ToFullPath("foo", ImmutableList<string>.Empty));

    [Test]
    public void ToFullPath_removes_the_leading_separator()
      => AreEqual("foo",
                  Keys.ToFullPath("/foo", ImmutableList<string>.Empty));

    [Test]
    public void ToFullPath_removes_the_leading_separator_for_any_scope()
      => AreEqual("foo",
                  Keys.ToFullPath("/foo", abcScope));

    [Test]
    public void ToFullPath_prepends_the_scope()
      => AreEqual("a/b/c/foo",
                  Keys.ToFullPath("foo", abcScope));

    [Test]
    public void ToFullPath_prepends_partial_scopes_when_backtracks_are_present()
      => AreEqual("a/foo",
                  Keys.ToFullPath("../../foo", abcScope));

    [Test]
    public void InterpretFromScope_returns_unchanged_absolute_keys()
      => AreEqual("/A", Keys.InterpretFromScope("/A", abcScope));

    [Test]
    public void InterpretFromScope_prepends_scope_to_relative_paths()
      => AreEqual("a/b/c/A", Keys.InterpretFromScope("A", abcScope));

    [Test]
    public void InterpretFromScope_returns_unchanged_relative_keys_when_scope_is_empty()
      => AreEqual("A", Keys.InterpretFromScope("A", ImmutableList<string>.Empty));

    [Test]
    public void InterpretFromScope_removes_all_backtracks_when_scope_size_matches_the_number_of_backtracks()
      => AreEqual("A", Keys.InterpretFromScope("../../../A", abcScope));

    [Test]
    public void InterpretFromScope_removes_some_backtracks_when_they_outnumber_the_scope_depth()
      => AreEqual("../A", Keys.InterpretFromScope("../../../../A", abcScope));

    [Test]
    public void InterpretFromScope_prepends_some_of_the_scope_when_scope_depth_outnumbers_backtracks()
      => AreEqual("a/b/A", Keys.InterpretFromScope("../A", abcScope));

    [Test]
    public void List_returns_empty_when_querying_unknown_key()
      => IsEmpty(Keys.List("a", new[] {"b"}));

    [Test]
    public void List_returns_single_key_when_path_is_absolute()
      => AreEqual(new[] { "a" },
                  Keys.List("/a", new[] {"a"}));

    [Test]
    public void List_returns_single_key_when_path_contains_no_wildcards()
      => AreEqual(new[] { "a" },
                  Keys.List("a", new[] {"a", "a/b"}));

    [Test]
    public void List_returns_all_keys_named_the_same_on_the_same_level()
      => AreEqual(new[] { "b/a" },
                  Keys.List("*/a", new[] {"a", "b/a", "c/b/a"}));

    [Test]
    public void List_returns_all_keys_named_the_same_on_any_level()
      => AreEqual(new[] { "a", "b/a", "c/b/a" },
                  Keys.List("**/a", new[] {"a", "b", "b/a", "b/b", "c/b/a", "c/b/b"}));

    [Test]
    public void List_does_not_treat_the_dot_as_a_regex_special_character()
      => IsEmpty(Keys.List("a.b", new[] {"axb"}));
  }
}