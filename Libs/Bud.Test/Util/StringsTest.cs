using NUnit.Framework;

namespace Bud.Util {
  public class StringsTest {
    [Test]
    public void ContainsAt_must_return_same_as_StartsWith_when_index_is_0()
      => Assert.AreEqual("foo".StartsWith("boo"), Strings.ContainsAt("foo", "boo", 0));

    [Test]
    public void ContainsAt_must_return_true_when_index_is_nonzero()
      => Assert.IsTrue(Strings.ContainsAt("foobar", "bar", 3));

    [Test]
    public void ContainsAt_must_return_false_when_index_is_nonzero()
      => Assert.IsFalse(Strings.ContainsAt("foobar", "boo", 3));

    [Test]
    public void ContainsAt_must_return_false_when_the_substring_goes_out_of_bounds()
      => Assert.IsFalse(Strings.ContainsAt("foobar", "bara", 3));

    [Test]
    public void ContainsAt_must_return_true_when_searching_for_an_empty_string()
      => Assert.IsTrue(Strings.ContainsAt("foobar", "", 4));
  }
}