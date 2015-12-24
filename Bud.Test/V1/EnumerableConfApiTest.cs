using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Bud.V1 {
  public class EnumerableConfApiTest {
    private readonly Key<int> numberKey = nameof(numberKey);

    #region IEnumerable

    private readonly Key<IEnumerable<int>> enumerableKey = nameof(enumerableKey);
    private readonly Key<IObservable<IEnumerable<int>>> observedEnumerableKey = nameof(observedEnumerableKey);

    [Test]
    public void Clear_resets_enumerables()
      => IsEmpty(Conf.Empty.InitValue(enumerableKey, new[] {1})
                     .Clear(enumerableKey)
                     .Get(enumerableKey));

    [Test]
    public void Add_adds_values_to_enumerables()
      => AreEqual(new[] {1, 2},
                  Conf.Empty.InitValue(enumerableKey, new[] {1})
                      .Add(enumerableKey, 2)
                      .Get(enumerableKey));

    [Test]
    public void Add_adds_computed_values_to_enumerables()
      => AreEqual(new[] {1, 43},
                  Conf.Empty.InitValue(numberKey, 42)
                      .InitValue(enumerableKey, new[] {1})
                      .Add(enumerableKey, c => numberKey[c] + 1)
                      .Get(enumerableKey));

    [Test]
    public void Add_adds_values_to_observed_enumerables()
      => AreEqual(new[] {1, 2},
                  Conf.Empty.InitValue(observedEnumerableKey, Observable.Return(new[] {1}))
                      .Add(observedEnumerableKey, 2)
                      .Get(observedEnumerableKey).Wait());

    #endregion

    #region IImmutableList

    private readonly Key<IImmutableList<int>> immutableListKey = nameof(immutableListKey);
    private readonly Key<IObservable<IImmutableList<int>>> observedIImmutableListKey = nameof(observedIImmutableListKey);

    [Test]
    public void Clear_resets_immutable_lists()
      => IsEmpty(Conf.Empty.InitValue(immutableListKey, ImmutableList.Create(1))
                     .Clear(immutableListKey)
                     .Get(immutableListKey));

    [Test]
    public void Add_adds_values_to_immutable_lists()
      => AreEqual(new[] {1, 2},
                  Conf.Empty.InitValue(immutableListKey, ImmutableList.Create(1))
                      .Add(immutableListKey, 2)
                      .Get(immutableListKey));

    [Test]
    public void Add_adds_computed_values_to_immutable_lists()
      => AreEqual(new[] {1, 43},
                  Conf.Empty.InitValue(numberKey, 42)
                      .InitValue(immutableListKey, ImmutableList.Create(1))
                      .Add(immutableListKey, c => numberKey[c] + 1)
                      .Get(immutableListKey));

    [Test]
    public void Add_adds_values_to_observed_immutable_lists()
      => AreEqual(new[] {1, 2},
                  Conf.Empty.InitValue(observedIImmutableListKey, Observable.Return(ImmutableList.Create(1)))
                      .Add(observedIImmutableListKey, 2)
                      .Get(observedIImmutableListKey).Wait());

    #endregion

    #region IImmutableSet

    private readonly Key<IImmutableSet<int>> immutableSetKey = nameof(immutableSetKey);
    private readonly Key<IObservable<IImmutableSet<int>>> observedImmutableSetKey = nameof(observedImmutableSetKey);

    [Test]
    public void Clear_resets_immutable_sets()
      => IsEmpty(Conf.Empty.InitValue(immutableListKey, ImmutableList.Create(1))
                     .Clear(immutableListKey)
                     .Get(immutableListKey));

    [Test]
    public void Add_adds_values_to_immutable_sets()
      => AreEqual(new[] {1, 2},
                  Conf.Empty.InitValue(immutableSetKey, ImmutableHashSet.Create(1))
                      .Add(immutableSetKey, 2)
                      .Get(immutableSetKey));

    [Test]
    public void Add_adds_computed_values_to_immutable_sets()
      => AreEqual(new[] {1, 43},
                  Conf.Empty.InitValue(numberKey, 42)
                      .InitValue(immutableSetKey, ImmutableHashSet.Create(1))
                      .Add(immutableSetKey, c => numberKey[c] + 1)
                      .Get(immutableSetKey));

    [Test]
    public void Add_adds_values_to_observed_immutable_sets()
      => AreEqual(new[] {1, 2},
                  Conf.Empty.InitValue(observedImmutableSetKey, Observable.Return(ImmutableHashSet.Create(1)))
                      .Add(observedImmutableSetKey, 2)
                      .Get(observedImmutableSetKey).Wait());

    #endregion
  }
}