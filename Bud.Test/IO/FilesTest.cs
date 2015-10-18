using System.Linq;
using NUnit.Framework;

namespace Bud.IO {
  public class FilesTest {
    [Test]
    public void Files_equals_to_the_initialising_enumeration() {
      var hashedFiles = new[] {"foo", "bar"};
      Assert.AreEqual(hashedFiles, new Files(hashedFiles));
    }

    [Test]
    public void Expanded_with_empty_files_produces_same_files() {
      var hashedFiles1 = new[] {"foo"};
      var hashedFiles2 = Enumerable.Empty<string>();
      Assert.AreEqual(hashedFiles1,
                      new Files(hashedFiles1).ExpandWith(new Files(hashedFiles2)));
    }

    [Test]
    public void Expanded_with_some_files_produces_a_concatenated_enumeration() {
      var hashedFiles1 = new[] {"foo"};
      var hashedFiles2 = new[] {"bar"};
      Assert.AreEqual(hashedFiles1.Concat(hashedFiles2),
                      new Files(hashedFiles1).ExpandWith(new Files(hashedFiles2)));
    }
  }
}