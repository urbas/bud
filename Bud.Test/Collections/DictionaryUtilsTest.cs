using System.Collections.Immutable;
using NUnit.Framework;
using static Bud.Collections.DictionaryUtils;
using static NUnit.Framework.Assert;

namespace Bud.Collections {
  public class DictionaryUtilsTest {
    [Test]
    public void DictionariesEqual_returns_true_when_both_are_empty()
      => IsTrue(DictionariesEqual(ImmutableDictionary<int, int>.Empty,
                                  ImmutableDictionary<int, int>.Empty));

    [Test]
    public void DictionariesEqual_returns_false_when_one_is_empty()
      => IsFalse(DictionariesEqual(ImmutableDictionary<int, int>.Empty,
                                   ImmutableDictionary<int, int>.Empty.Add(1, 1)));

    [Test]
    public void DictionariesEqual_returns_false_when_same_size_but_different_keys()
      => IsFalse(DictionariesEqual(ImmutableDictionary<int, int>.Empty.Add(2, 1),
                                   ImmutableDictionary<int, int>.Empty.Add(1, 1)));

    [Test]
    public void DictionariesEqual_returns_false_when_same_keys_but_different_values()
      => IsFalse(DictionariesEqual(ImmutableDictionary<int, int>.Empty.Add(1, 1),
                                   ImmutableDictionary<int, int>.Empty.Add(1, 2)));
  }
}