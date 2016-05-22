using System.Collections.Generic;
using System.Linq;
using static Bud.Option;

namespace Bud.Collections {
  public static class EnumerableUtils {
    public static int ElementwiseHashCode<TElement>(IEnumerable<TElement> enumerable)
      => enumerable?.Aggregate(487, MergeHash) ?? 0;

    public static Option<TSource> TryGetFirst<TSource>(this IEnumerable<TSource> source) {
      if (source == null) {
        return None<TSource>();
      }
      var list = source as IList<TSource>;
      if (list != null) {
        if (list.Count > 0) {
          return list[0];
        }
      } else {
        using (var e = source.GetEnumerator()) {
          if (e.MoveNext()) {
            return e.Current;
          }
        }
      }
      return None<TSource>();
    }

    private static int MergeHash<TElement>(int hashCode, TElement element) {
      unchecked {
        return hashCode*31 + element.GetHashCode();
      }
    }
  }
}