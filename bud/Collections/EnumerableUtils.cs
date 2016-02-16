using System.Collections.Generic;
using System.Linq;

namespace Bud.Collections {
  public static class EnumerableUtils {
    public static int ElementwiseHashCode<TElement>(IEnumerable<TElement> enumerable)
      => enumerable?.Aggregate(487, MergeHash) ?? 0;

    private static int MergeHash<TElement>(int hashCode, TElement element) {
      unchecked {
        return hashCode * 31 + element.GetHashCode();
      }
    }
  }
}