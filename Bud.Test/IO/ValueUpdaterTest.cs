using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NUnit.Framework;
using static System.Linq.Enumerable;
using static NUnit.Framework.Assert;

namespace Bud.IO {
  public class ValueUpdaterTest {
    private TestValueStore valueStore;
    private ValueUpdater<Timestamped<int>, string> cachedBuilder;
    private Func<Timestamped<int>, string> valueFactory;
    private List<Timestamped<int>> createdValues;
    private Timestamped<int> int1At0 = Timestamped.Create(1, 0);
    private Timestamped<int> int1At1 = Timestamped.Create(1, 1);

    [SetUp]
    public void SetUp() {
      valueStore = new TestValueStore();
      valueFactory = ValueFactory;
      cachedBuilder = new ValueUpdater<Timestamped<int>, string>(
        valueStore,
        valueFactory);
      createdValues = new List<Timestamped<int>>();
    }

    private string ValueFactory(Timestamped<int> i) {
      createdValues.Add(i);
      return i.ToString();
    }

    [Test]
    public void Does_not_interact_with_the_store_when_there_are_no_changes() {
      cachedBuilder.UpdateWith(Empty<Timestamped<int>>());
      AreEqual(0, valueStore.AddCalls);
      AreEqual(0, valueStore.RemoveCalls);
      IsEmpty(createdValues);
    }

    [Test]
    public void Adds_single_value_to_the_store() {
      cachedBuilder.UpdateWith(new[] {int1At0});
      AreEqual(1, valueStore.AddCalls);
      AreEqual(0, valueStore.RemoveCalls);
      AreEqual(new[] {int1At0.ToString()}, valueStore.Adds);
      AreEqual(Empty<string>(), valueStore.Removes);
      AreEqual(new[] {int1At0}, createdValues);
    }

    [Test]
    public void Removes_the_value_from_the_store() {
      Adds_single_value_to_the_store();
      cachedBuilder.UpdateWith(Empty<Timestamped<int>>());
      AreEqual(1, valueStore.AddCalls);
      AreEqual(1, valueStore.RemoveCalls);
      AreEqual(new[] {int1At0.ToString()}, valueStore.Adds);
      AreEqual(new[] {int1At0.ToString()}, valueStore.Removes);
      AreEqual(new[] {int1At0}, createdValues);
    }

    [Test]
    public void Changes_the_output_in_the_store_without_rebuilding_the_old_value() {
      Adds_single_value_to_the_store();
      cachedBuilder.UpdateWith(new[] {int1At1});
      AreEqual(2, valueStore.AddCalls);
      AreEqual(1, valueStore.RemoveCalls);
      AreEqual(new[] {int1At0.ToString(), int1At1.ToString()}, valueStore.Adds);
      AreEqual(new[] {int1At0.ToString()}, valueStore.Removes);
      AreEqual(new[] {int1At0, int1At1}, createdValues);
    }

    public class TestValueStore : IValueStore<string> {
      public IImmutableList<string> Adds { get; private set; }
      public IImmutableList<string> Removes { get; private set; }
      public int AddCalls { get; private set; }
      public int RemoveCalls { get; private set; }

      public TestValueStore() {
        Adds = ImmutableList<string>.Empty;
        Removes = ImmutableList<string>.Empty;
      }

      public void Add(IEnumerable<string> newValues) {
        Adds = Adds.AddRange(newValues);
        ++AddCalls;
      }

      public void Remove(IEnumerable<string> oldValues) {
        Removes = Removes.AddRange(oldValues);
        ++RemoveCalls;
      }
    }
  }
}