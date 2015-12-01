using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Bud.Collections;

namespace Bud.IO {
  public class Diff<T> : IDiff<T> {
    private readonly Lazy<int> cachedHashCode;

    public Diff(IImmutableSet<T> added,
                IImmutableSet<T> removed,
                IImmutableSet<T> changed,
                IImmutableSet<T> all) {
      Added = added;
      Removed = removed;
      Changed = changed;
      All = all;
      cachedHashCode = new Lazy<int>(CalculateHashCode);
    }

    public IImmutableSet<T> Added { get; }
    public IImmutableSet<T> Removed { get; }
    public IImmutableSet<T> Changed { get; }
    public IImmutableSet<T> All { get; }

    protected bool Equals(IDiff<T> other)
      => Added.SetEquals(other.Added) &&
         Removed.SetEquals(other.Removed) &&
         Changed.SetEquals(other.Changed) &&
         All.SetEquals(other.All);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj is IDiff<T> && Equals((IDiff<T>) obj);
    }

    public override int GetHashCode() => cachedHashCode.Value;

    public static bool operator ==(Diff<T> left, IDiff<T> right) => Equals(left, right);

    public static bool operator !=(Diff<T> left, IDiff<T> right) => !Equals(left, right);

    public override string ToString()
      => $"Added: [{string.Join(", ", Added)}], Removed: [{string.Join(", ", Removed)}], Changed: [{string.Join(", ", Changed)}], Unchanged: [{string.Join(", ", All.Except(Added).Except(Changed))}]";

    public string ToPrettyString() {
      var sb = new StringBuilder();
      foreach (var source in Added) {
        sb.AppendLine($"+ {source}");
      }
      foreach (var source in Removed) {
        sb.AppendLine($"- {source}");
      }
      foreach (var source in Changed) {
        sb.AppendLine($"~ {source}");
      }
      return sb.ToString();
    }

    private int CalculateHashCode() {
      unchecked {
        var hashCode = EnumerableUtils.ElementwiseHashCode(Added);
        hashCode = (hashCode * 397) ^ EnumerableUtils.ElementwiseHashCode(Removed);
        hashCode = (hashCode * 397) ^ EnumerableUtils.ElementwiseHashCode(Changed);
        hashCode = (hashCode * 397) ^ EnumerableUtils.ElementwiseHashCode(All);
        return hashCode;
      }
    }
  }

  public static class Diff {
    public static Diff<T> Empty<T>() => EmptyDiff<T>.Instance;

    public static Diff<T> DiffByTimestamp<T>(this IDiff<T> previousDiff, IEnumerable<T> timestampedElements) where T : ITimestamped {
      var all = timestampedElements.ToImmutableHashSet();
      var removed = previousDiff.All.Except(all);
      var added = all.Except(previousDiff.All);
      var changed = all.Except(added)
                       .Where(el => HasElementChanged(previousDiff.All, el))
                       .ToImmutableHashSet();
      return new Diff<T>(added, removed, changed, all);
    }

    private static bool HasElementChanged<T>(IImmutableSet<T> previousDiff, T el) where T : ITimestamped {
      T oldEl;
      return previousDiff.TryGetValue(el, out oldEl) && el.Timestamp > oldEl.Timestamp;
    }

    private static class EmptyDiff<T> {
      public static readonly Diff<T> Instance = new Diff<T>(ImmutableHashSet<T>.Empty, ImmutableHashSet<T>.Empty, ImmutableHashSet<T>.Empty, ImmutableHashSet<T>.Empty);
    }

    public static ImmutableDictionary<TKey, TValue> UpdateCache<TKey, TValue>(ImmutableDictionary<TKey, TValue> cache,
                                                                              IDiff<TKey> diff,
                                                                              Func<TKey, TValue> valueFactory) {
      cache = cache.AddRange(diff.Added.Select(key => new KeyValuePair<TKey, TValue>(key, valueFactory(key))));
      cache = cache.RemoveRange(diff.Removed);
      return cache.SetItems(diff.Changed.Select(key => new KeyValuePair<TKey, TValue>(key, valueFactory(key))));
    }
  }
}