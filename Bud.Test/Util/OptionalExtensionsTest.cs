using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bud.Util {
  public class OptionalExtensionsTest {
    [Test]
    public void GetOrElse_returns_the_contained_value()
      => Assert.AreEqual(42, new Optional<int>(42).GetOrElse(9001));

    [Test]
    public void GetOrElse_returns_the_default_value()
      => Assert.AreEqual(9001, new Optional<int>().GetOrElse(9001));
  }
}