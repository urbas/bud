using System.Collections.Immutable;
using NUnit.Framework;
using static Bud.Collections.EnumerableUtils;

namespace Bud.Collections {
  public class EnumerableUtilsTest {
    [Test]
    public void ElementwiseHashCode_must_equal_for_equal_collections() {
      var collection1 = new[] {42, 9001};
      var collection2 = ImmutableArray.CreateRange(collection1);

      Assert.AreEqual(collection1, collection2);
      Assert.AreEqual(ElementwiseHashCode(collection1), ElementwiseHashCode(collection2));
    }
  }
}