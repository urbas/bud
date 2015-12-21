using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using NUnit.Framework;

namespace Bud.V1 {
  public class EnumerableConfApiTest {
    private readonly Key<int> numberKey = nameof(numberKey);
    private readonly Key<IEnumerable<int>> enumerableKey = nameof(enumerableKey);
    private readonly Key<IObservable<IEnumerable<int>>> observedEnumerableKey = nameof(observedEnumerableKey);
    private readonly Key<IImmutableList<int>> immutableListKey = nameof(immutableListKey);
    private readonly Key<IImmutableSet<int>> immutableSetKey = nameof(immutableSetKey);

    [Test]
    public void Add_adds_values_to_enumerables()
      => Assert.AreEqual(new[] {1, 2},
                         Conf.Empty.InitValue(enumerableKey, new[] {1})
                             .Add(enumerableKey, 2)
                             .Get(enumerableKey));

    [Test]
    public void Add_adds_computed_values_to_enumerables()
      => Assert.AreEqual(new[] {1, 43},
                         Conf.Empty.InitValue(numberKey, 42)
                             .InitValue(enumerableKey, new[] {1})
                             .Add(enumerableKey, c => numberKey[c] + 1)
                             .Get(enumerableKey));

    [Test]
    public void Merge_adds_values_to_observed_enumerables()
      => Assert.AreEqual(new[] { 1, 2 },
                         Conf.Empty.InitValue(observedEnumerableKey, Observable.Return(new[] { 1 }))
                             .Merge(observedEnumerableKey, Observable.Return(2))
                             .Get(observedEnumerableKey).Wait());

    [Test]
    public void Add_adds_values_to_immutable_lists()
      => Assert.AreEqual(new[] {1, 2},
                         Conf.Empty.InitValue(immutableListKey, ImmutableList.Create(1))
                             .Add(immutableListKey, 2)
                             .Get(immutableListKey));

    [Test]
    public void Add_adds_computed_values_to_immutable_lists()
      => Assert.AreEqual(new[] { 1, 43 },
                         Conf.Empty.InitValue(numberKey, 42)
                             .InitValue(immutableListKey, ImmutableList.Create(1))
                             .Add(immutableListKey, c => numberKey[c] + 1)
                             .Get(immutableListKey));

    [Test]
    public void Add_adds_values_to_immutable_sets()
      => Assert.AreEqual(new[] {1, 2},
                         Conf.Empty.InitValue(immutableSetKey, ImmutableHashSet.Create(1))
                             .Add(immutableSetKey, 2)
                             .Get(immutableSetKey));

    [Test]
    public void Add_adds_computed_values_to_immutable_sets()
      => Assert.AreEqual(new[] { 1, 43 },
                         Conf.Empty.InitValue(numberKey, 42)
                             .InitValue(immutableSetKey, ImmutableHashSet.Create(1))
                             .Add(immutableSetKey, c => numberKey[c] + 1)
                             .Get(immutableSetKey));
  }
}