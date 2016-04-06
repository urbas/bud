using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using static Bud.Collections.EnumerableUtils;
using static Bud.Util.Option;
using static NUnit.Framework.Assert;

namespace Bud.Collections {
  public class EnumerableUtilsTest {
    [Test]
    public void ElementwiseHashCode_must_equal_for_equal_collections() {
      var collection1 = new[] {42, 9001};
      var collection2 = ImmutableArray.CreateRange(collection1);

      AreEqual(collection1, collection2);
      AreEqual(ElementwiseHashCode(collection1), ElementwiseHashCode(collection2));
    }

    [Test]
    public void TryGetFirst_returns_none_when_enumerable_is_empty()
      => AreEqual(None<int>(), Enumerable.Empty<int>().TryGetFirst());

    [Test]
    public void TryGetFirst_returns_the_first_element_when_enumerable_is_not_empty()
      => AreEqual(Some(42), new [] {42, 1}.TryGetFirst());

    [Test]
    public void TryGetFirst_returns_None_when_given_null()
      => AreEqual(None<int>(), (null as IEnumerable<int>).TryGetFirst());
  }
}