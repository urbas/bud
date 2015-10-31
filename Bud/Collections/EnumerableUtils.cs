using System.Collections.Generic;
using System.Linq;

namespace Bud.Collections {
  public static class EnumerableUtils {
    public static int ElementwiseHashCode<TElement>(IEnumerable<TElement> immutableArray) {
      unchecked {
        if (immutableArray != null) {
          return immutableArray.Aggregate(487, (hashCode, element) => hashCode * 31 + element.GetHashCode());
        }
        return 0;
      }
    }
  }
}