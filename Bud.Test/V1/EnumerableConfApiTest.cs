using System.Collections.Generic;
using System.Collections.Immutable;
using NUnit.Framework;

namespace Bud.V1 {
  public class EnumerableConfApiTest {
    private readonly Key<IEnumerable<int>> enumerableKey = nameof(enumerableKey);
    private readonly Key<IImmutableList<int>> immutableListKey = nameof(immutableListKey);
    private readonly Key<IImmutableSet<int>> immutableSetKey = nameof(immutableSetKey);

    [Test]
    public void Add_adds_values_to_enumerables()
      => Assert.AreEqual(new[] {1, 2},
                         Conf.Empty.InitValue(enumerableKey, new[] {1})
                             .Add(enumerableKey, 2)
                             .Get(enumerableKey));

    [Test]
    public void Add_adds_values_to_immutable_lists()
      => Assert.AreEqual(new[] {1, 2},
                         Conf.Empty.InitValue(immutableListKey, ImmutableList.Create(1))
                             .Add(immutableListKey, 2)
                             .Get(immutableListKey));

    [Test]
    public void Add_adds_values_to_immutable_sets()
      => Assert.AreEqual(new[] {1, 2},
                         Conf.Empty.InitValue(immutableSetKey, ImmutableHashSet.Create(1))
                             .Add(immutableSetKey, 2)
                             .Get(immutableSetKey));
  }
}