using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Bud.IO {
  public class Diff<T> where T : IHashed {
    public Diff(ImmutableHashSet<T> added, ImmutableHashSet<T> removed, ImmutableHashSet<T> changed, ImmutableHashSet<T> all) {
      Added = added;
      Removed = removed;
      Changed = changed;
      All = all;
    }

    public ImmutableHashSet<T> Added { get; }
    public ImmutableHashSet<T> Removed { get; }
    public ImmutableHashSet<T> Changed { get; }
    public ImmutableHashSet<T> All { get; }

    protected bool Equals(Diff<T> other) => Added.SetEquals(other.Added) &&
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
      return obj.GetType() == typeof(Diff<T>) && Equals((Diff<T>) obj);
    }

    public override int GetHashCode() {
      unchecked {
        var hashCode = Added.GetHashCode();
        hashCode = (hashCode * 397) ^ Removed.GetHashCode();
        hashCode = (hashCode * 397) ^ Changed.GetHashCode();
        hashCode = (hashCode * 397) ^ All.GetHashCode();
        return hashCode;
      }
    }

    public static bool operator ==(Diff<T> left, Diff<T> right) => Equals(left, right);

    public static bool operator !=(Diff<T> left, Diff<T> right) => !Equals(left, right);

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
  }

  public static class Diff {
    public static Diff<T> Empty<T>() where T : IHashed => EmptyDiff<T>.Instance;

    public static Diff<T> NextDiff<T>(this Diff<T> previousDiff, IEnumerable<T> timestampedElements) where T : IHashed {
      var timestampedElementsList = timestampedElements as IList<T> ?? timestampedElements.ToList();
      var all = timestampedElementsList.ToImmutableHashSet();
      var removed = previousDiff.All.Except(all);
      var added = all.Except(previousDiff.All);
      var changed = all.Except(added).Where(el => HasElementChanged(previousDiff, el)).ToImmutableHashSet();
      return new Diff<T>(added, removed, changed, all);
    }

    private static bool HasElementChanged<T>(Diff<T> previousDiff, T el) where T : IHashed {
      T oldEl;
      return previousDiff.All.TryGetValue(el, out oldEl) && el.Hash > oldEl.Hash;
    }

    private static class EmptyDiff<T> where T : IHashed {
      public static readonly Diff<T> Instance = new Diff<T>(ImmutableHashSet<T>.Empty, ImmutableHashSet<T>.Empty, ImmutableHashSet<T>.Empty, ImmutableHashSet<T>.Empty);
    }
  }
}