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

    [Test]
    public void InitEmpty_initializes_enumerables_to_empty()
      => IsEmpty(Conf.Empty.InitEmpty(enumerableKey).Get(enumerableKey));

    [Test]
    public void InitEmpty_does_not_changed_initialized_enumerables()
      => AreEqual(new[] {1},
                  Conf.Empty.Init(enumerableKey, new[] {1})
                      .InitEmpty(enumerableKey)
                      .Get(enumerableKey));

    [Test]
    public void Clear_resets_enumerables()
      => IsEmpty(Conf.Empty.Init(enumerableKey, new[] {1})
                     .Clear(enumerableKey)
                     .Get(enumerableKey));

    [Test]
    public void Add_adds_values_to_enumerables()
      => AreEqual(new[] {1, 2},
                  Conf.Empty.Init(enumerableKey, new[] {1})
                      .Add(enumerableKey, 2)
                      .Get(enumerableKey));

    [Test]
    public void Add_adds_computed_values_to_enumerables()
      => AreEqual(new[] {1, 43},
                  Conf.Empty.Init(numberKey, 42)
                      .Init(enumerableKey, new[] {1})
                      .Add(enumerableKey, c => numberKey[c] + 1)
                      .Get(enumerableKey));

    #endregion

    #region IObservable<IEnumerable<T>>

    private readonly Key<IObservable<IEnumerable<int>>> observedEnumerableKey = nameof(observedEnumerableKey);

    [Test]
    public void InitEmpty_initializes_observed_enumerables_to_empty()
      => That(Conf.Empty.InitEmpty(observedEnumerableKey).Get(observedEnumerableKey).ToEnumerable(),
              Has.Exactly(1).Empty);

    [Test]
    public void InitEmpty_does_not_changed_initialized_observed_enumerables()
      => That(Conf.Empty.Init(observedEnumerableKey, Observable.Return(new[] {1}))
                  .InitEmpty(observedEnumerableKey)
                  .Get(observedEnumerableKey)
                  .ToEnumerable(),
              Has.Exactly(1).EqualTo(new[] {1}));

    [Test]
    public void Clear_resets_ovbserved_enumerables()
      => That(Conf.Empty.Init(observedEnumerableKey, Observable.Return(new[] {1}))
                  .Clear(observedEnumerableKey)
                  .Get(observedEnumerableKey)
                  .ToEnumerable(),
              Has.Exactly(1).Empty);

    [Test]
    public void Add_adds_values_to_observed_enumerables()
      => AreEqual(new[] {1, 2, 3},
                  Conf.Empty.Init(observedEnumerableKey, Observable.Return(new[] {1}))
                      .Add(observedEnumerableKey, 2, 3)
                      .Get(observedEnumerableKey).Wait());

    [Test]
    public void Add_adds_observed_value_to_observed_enumerables()
      => AreEqual(new[] {1, 2},
                  Conf.Empty.Init(observedEnumerableKey, Observable.Return(new[] {1}))
                      .Add(observedEnumerableKey, Observable.Return(2))
                      .Get(observedEnumerableKey).Wait());

    [Test]
    public void Add_adds_observed_values_to_observed_enumerables()
      => AreEqual(new[] {1, 2, 3},
                  Conf.Empty.Init(observedEnumerableKey, Observable.Return(new[] {1}))
                      .Add(observedEnumerableKey, Observable.Return(new [] {2, 3}))
                      .Get(observedEnumerableKey).Wait());

    #endregion

    #region IImmutableList

    private readonly Key<IImmutableList<int>> immutableListKey = nameof(immutableListKey);

    [Test]
    public void InitEmpty_initializes_immutable_lists_to_empty()
      => IsEmpty(Conf.Empty.InitEmpty(immutableListKey).Get(immutableListKey));

    [Test]
    public void InitEmpty_does_not_changed_initialized_immutable_lists()
      => AreEqual(new[] {1},
                  Conf.Empty.Init(immutableListKey, ImmutableList.Create(1))
                      .InitEmpty(immutableListKey)
                      .Get(immutableListKey));

    [Test]
    public void Clear_resets_immutable_lists()
      => IsEmpty(Conf.Empty.Init(immutableListKey, ImmutableList.Create(1))
                     .Clear(immutableListKey)
                     .Get(immutableListKey));

    [Test]
    public void Add_adds_values_to_immutable_lists()
      => AreEqual(new[] {1, 2},
                  Conf.Empty.Init(immutableListKey, ImmutableList.Create(1))
                      .Add(immutableListKey, 2)
                      .Get(immutableListKey));

    [Test]
    public void Add_adds_computed_values_to_immutable_lists()
      => AreEqual(new[] {1, 43},
                  Conf.Empty.Init(numberKey, 42)
                      .Init(immutableListKey, ImmutableList.Create(1))
                      .Add(immutableListKey, c => numberKey[c] + 1)
                      .Get(immutableListKey));

    #endregion

    #region IObservable<IImmutableList<T>>

    private readonly Key<IObservable<IImmutableList<int>>> observedIImmutableListKey = nameof(observedIImmutableListKey);

    [Test]
    public void InitEmpty_initializes_observed_immutable_lists_to_empty()
      => That(Conf.Empty.InitEmpty(observedIImmutableListKey).Get(observedIImmutableListKey).ToEnumerable(),
              Has.Exactly(1).Empty);

    [Test]
    public void InitEmpty_does_not_changed_initialized_observed_immutable_lists()
      => That(Conf.Empty.Init(observedIImmutableListKey, Observable.Return(ImmutableList.Create(1)))
                  .InitEmpty(observedIImmutableListKey)
                  .Get(observedIImmutableListKey)
                  .ToEnumerable(),
              Has.Exactly(1).EqualTo(new[] { 1 }));

    [Test]
    public void Clear_resets_ovbserved_immutable_lists()
      => That(Conf.Empty.Init(observedIImmutableListKey, Observable.Return(ImmutableList.Create(1)))
                  .Clear(observedIImmutableListKey)
                  .Get(observedIImmutableListKey)
                  .ToEnumerable(),
              Has.Exactly(1).Empty);

    [Test]
    public void Add_adds_values_to_observed_immutable_lists()
      => AreEqual(new[] { 1, 2, 3 },
                  Conf.Empty.Init(observedIImmutableListKey, Observable.Return(ImmutableList.Create(1)))
                      .Add(observedIImmutableListKey, 2, 3)
                      .Get(observedIImmutableListKey).Wait());

    [Test]
    public void Add_adds_observed_value_to_observed_immutable_lists()
      => AreEqual(new[] { 1, 2 },
                  Conf.Empty.Init(observedIImmutableListKey, Observable.Return(ImmutableList.Create(1)))
                      .Add(observedIImmutableListKey, Observable.Return(2))
                      .Get(observedIImmutableListKey).Wait());

    [Test]
    public void Add_adds_observed_values_to_observed_immutable_lists()
      => AreEqual(new[] { 1, 2, 3 },
                  Conf.Empty.Init(observedIImmutableListKey, Observable.Return(ImmutableList.Create(1)))
                      .Add(observedIImmutableListKey, Observable.Return(new[] { 2, 3 }))
                      .Get(observedIImmutableListKey).Wait());

    #endregion

    #region IImmutableSet

    private readonly Key<IImmutableSet<int>> immutableSetKey = nameof(immutableSetKey);

    [Test]
    public void InitEmpty_initializes_immutable_sets_to_empty()
      => IsEmpty(Conf.Empty.InitEmpty(immutableSetKey).Get(immutableSetKey));

    [Test]
    public void InitEmpty_does_not_changed_initialized_immutable_sets()
      => AreEqual(new[] {1},
                  Conf.Empty.Init(immutableSetKey, ImmutableHashSet.Create(1))
                      .InitEmpty(immutableSetKey)
                      .Get(immutableSetKey));

    [Test]
    public void Clear_resets_immutable_sets()
      => IsEmpty(Conf.Empty.Init(immutableSetKey, ImmutableHashSet.Create(1))
                     .Clear(immutableSetKey)
                     .Get(immutableSetKey));

    [Test]
    public void Add_adds_values_to_immutable_sets()
      => AreEqual(new[] {1, 2},
                  Conf.Empty.Init(immutableSetKey, ImmutableHashSet.Create(1))
                      .Add(immutableSetKey, 2)
                      .Get(immutableSetKey));

    [Test]
    public void Add_adds_computed_values_to_immutable_sets()
      => AreEqual(new[] {1, 43},
                  Conf.Empty.Init(numberKey, 42)
                      .Init(immutableSetKey, ImmutableHashSet.Create(1))
                      .Add(immutableSetKey, c => numberKey[c] + 1)
                      .Get(immutableSetKey));

    #endregion

    #region IObservable<IImmutableSet<T>>

    private readonly Key<IObservable<IImmutableSet<int>>> observedImmutableSetKey = nameof(observedImmutableSetKey);

    [Test]
    public void InitEmpty_initializes_observed_immutable_sets_to_empty()
      => That(Conf.Empty.InitEmpty(observedImmutableSetKey).Get(observedImmutableSetKey).ToEnumerable(),
              Has.Exactly(1).Empty);

    [Test]
    public void InitEmpty_does_not_changed_initialized_observed_immutable_sets()
      => That(Conf.Empty.Init(observedImmutableSetKey, Observable.Return(ImmutableHashSet.Create(1)))
                  .InitEmpty(observedImmutableSetKey)
                  .Get(observedImmutableSetKey)
                  .ToEnumerable(),
              Has.Exactly(1).EqualTo(new[] { 1 }));

    [Test]
    public void Clear_resets_ovbserved_immutable_sets()
      => That(Conf.Empty.Init(observedImmutableSetKey, Observable.Return(ImmutableHashSet.Create(1)))
                  .Clear(observedImmutableSetKey)
                  .Get(observedImmutableSetKey)
                  .ToEnumerable(),
              Has.Exactly(1).Empty);

    [Test]
    public void Add_adds_values_to_observed_immutable_sets()
      => AreEqual(new[] { 1, 2, 3 },
                  Conf.Empty.Init(observedImmutableSetKey, Observable.Return(ImmutableHashSet.Create(1)))
                      .Add(observedImmutableSetKey, 2, 3)
                      .Get(observedImmutableSetKey).Wait());

    [Test]
    public void Add_adds_observed_value_to_observed_immutable_sets()
      => AreEqual(new[] { 1, 2 },
                  Conf.Empty.Init(observedImmutableSetKey, Observable.Return(ImmutableHashSet.Create(1)))
                      .Add(observedImmutableSetKey, Observable.Return(2))
                      .Get(observedImmutableSetKey).Wait());

    [Test]
    public void Add_adds_observed_values_to_observed_immutable_sets()
      => AreEqual(new[] { 1, 2, 3 },
                  Conf.Empty.Init(observedImmutableSetKey, Observable.Return(ImmutableHashSet.Create(1)))
                      .Add(observedImmutableSetKey, Observable.Return(new[] { 2, 3 }))
                      .Get(observedImmutableSetKey).Wait());

    #endregion
  }
}