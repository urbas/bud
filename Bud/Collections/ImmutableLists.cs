using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bud.Collections {
  public static class ImmutableLists {
    public static ImmutableList<T> FlattenToImmutableList<T>(this IEnumerable<IEnumerable<T>> resources)
      => resources.Aggregate(ImmutableList.CreateBuilder<T>(),
                             (builder, enumerable) => {
                               builder.AddRange(enumerable);
                               return builder;
                             })
                  .ToImmutable();
  }
}