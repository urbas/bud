using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Text;

namespace Bud.Pipeline {
  public class Diff<T> {
    public Diff(ImmutableHashSet<T> added, ImmutableHashSet<T> removed, ImmutableHashSet<T> changed, ImmutableHashSet<T> all, ImmutableDictionary<T, DateTimeOffset> timestamps) {
      Added = added;
      Removed = removed;
      Changed = changed;
      All = all;
      Timestamps = timestamps;
    }

    public ImmutableHashSet<T> Added { get; }
    public ImmutableHashSet<T> Removed { get; }
    public ImmutableHashSet<T> Changed { get; }
    public ImmutableHashSet<T> All { get; }
    public ImmutableDictionary<T, DateTimeOffset> Timestamps { get; }

    protected bool Equals(Diff<T> other) => Added.SequenceEqual(other.Added) &&
                                            Removed.SequenceEqual(other.Removed) &&
                                            Changed.SequenceEqual(other.Changed) &&
                                            All.SequenceEqual(other.All) && 
                                            Timestamps.SequenceEqual(other.Timestamps);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == typeof(Diff<T>) && Equals((Diff<T>) obj);
    }

    public override int GetHashCode() {
      unchecked {
        var hashCode = Added.GetHashCode();
        hashCode = (hashCode * 397) ^ Removed.GetHashCode();
        hashCode = (hashCode * 397) ^ Changed.GetHashCode();
        hashCode = (hashCode * 397) ^ All.GetHashCode();
        hashCode = (hashCode * 397) ^ Timestamps.GetHashCode();
        return hashCode;
      }
    }

    public static bool operator ==(Diff<T> left, Diff<T> right) => Equals(left, right);

    public static bool operator !=(Diff<T> left, Diff<T> right) => !Equals(left, right);

    public override string ToString()
      => $"Added: [{string.Join(", ", Added)}], Removed: [{string.Join(", ", Removed)}], Changed: [{string.Join(", ", Changed)}], Unchanged: [{string.Join(", ", All.Except(Added).Except(Changed))}], Timestamps: [{string.Join(", ", Timestamps.Select(pair => $"({pair.Key}, {pair.Value})"))}]";

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
  }

  public static class Diff {
    public static Diff<T> Empty<T>() => EmptyDiff<T>.Instance;

    public static Diff<T> NextDiff<T>(this Diff<T> previousDiff, IEnumerable<Timestamped<T>> timestampedElements) {
      var timestampedElementsList = timestampedElements as IList<Timestamped<T>> ?? timestampedElements.ToList();
      var all = timestampedElementsList.Select(e => e.Value).ToImmutableHashSet();
      var timestamps = ImmutableDictionary.CreateRange(timestampedElementsList.Select(s => new KeyValuePair<T, DateTimeOffset>(s.Value, s.Timestamp)));
      var removed = previousDiff.All.Except(all);
      var added = all.Except(previousDiff.All);
      var changed = all.Except(added).Where(el => timestamps[el] > previousDiff.Timestamps[el]).ToImmutableHashSet();
      return new Diff<T>(added, removed, changed, all, timestamps);
    }

    private static class EmptyDiff<T> {
      public static readonly Diff<T> Instance = new Diff<T>(ImmutableHashSet<T>.Empty, ImmutableHashSet<T>.Empty, ImmutableHashSet<T>.Empty, ImmutableHashSet<T>.Empty, ImmutableDictionary<T, DateTimeOffset>.Empty);
    }
  }
}