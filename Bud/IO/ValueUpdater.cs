using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bud.IO {
  public class ValueUpdater<TInput, TOutput> where TInput : ITimestamped {
    public IValueStore<TOutput> ValueStore { get; }

    private readonly Func<TInput, TOutput> valueFactory;

    public Diff<TInput> LastDiff { get; private set; }

    private IImmutableDictionary<TInput, TOutput> valueCache
      = ImmutableDictionary<TInput, TOutput>.Empty;

    public ValueUpdater(IValueStore<TOutput> valueStore,
                               Func<TInput, TOutput> valueFactory) {
      ValueStore = valueStore;
      this.valueFactory = valueFactory;
      LastDiff = Diff.Empty<TInput>();
    }

    public void UpdateWith(IEnumerable<TInput> inputDiff) {
      LastDiff = LastDiff.DiffByTimestamp(inputDiff);
      var oldValueCache = valueCache;
      valueCache = Diff.UpdateCache(oldValueCache, LastDiff, valueFactory);
      if (LastDiff.Added.Count > 0) {
        ValueStore.Add(LastDiff.Added.Select(i => valueCache[i]));
      }
      if (LastDiff.Removed.Count > 0) {
        ValueStore.Remove(LastDiff.Removed.Select(i => oldValueCache[i]));
      }
      if (LastDiff.Changed.Count > 0) {
        ValueStore.Remove(LastDiff.Changed.Select(i => oldValueCache[i]));
        ValueStore.Add(LastDiff.Changed.Select(i => valueCache[i]));
      }
    }
  }
}