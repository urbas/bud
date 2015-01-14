using NUnit.Framework;

namespace Bud.IO {
  public class PathsTest {
    [Test]
    public void ToAgnosticPath_MUST_return_null_WHEN_given_null() {
      Assert.AreEqual(null, Paths.ToAgnosticPath(null));
    }

    [Test]
    public void ToAgnosticPath_MUST_return_the_empty_string_WHEN_given_an_empty_string() {
      Assert.AreEqual(string.Empty, Paths.ToAgnosticPath(string.Empty));
    }

    [Test]
    public void ToAgnosticPath_MUST_replace_backward_slashes_with_forward_slashes() {
      Assert.AreEqual("a/b/c", Paths.ToAgnosticPath(@"a\b\c"));
    }
  }
}