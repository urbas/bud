using System.Linq;
using NUnit.Framework;

namespace Bud.IO {
  public class FilesTest {
    [Test]
    public void Files_equals_to_the_initialising_enumeration() {
      var files = new[] {"foo", "bar"};
      Assert.AreEqual(files, new Files(files));
    }

    [Test]
    public void Expanded_with_empty_files_produces_same_files() {
      var files1 = new[] {"foo"};
      var files2 = Enumerable.Empty<string>();
      Assert.AreEqual(files1,
                      new Files(files1).ExpandWith(new Files(files2)));
    }

    [Test]
    public void Expanded_with_some_files_produces_a_concatenated_enumeration() {
      var files1 = new[] {"foo"};
      var files2 = new[] {"bar"};
      Assert.AreEqual(files1.Concat(files2),
                      new Files(files1).ExpandWith(new Files(files2)));
    }
  }
}