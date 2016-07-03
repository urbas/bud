using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Bud.Collections {
  public class ImmutableListsTest {
    [Test]
    public void FlattenToImmutableList_returns_empty()
      => IsEmpty(Enumerable.Empty<IEnumerable<int>>().FlattenToImmutableList());

    [Test]
    public void FlattenToImmutableList_returns_some_elements()
      => AreEqual(new[] {1, 2, 42},
                  new[] {new[] {1, 2}, new[] {42}}.FlattenToImmutableList());
  }
}