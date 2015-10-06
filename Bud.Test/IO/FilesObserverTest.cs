using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Bud.IO {
  public class FilesObserverTest {
    private TemporaryDirectory tempDir;

    [SetUp]
    public void SetUp() => tempDir = new TemporaryDirectory();

    [TearDown]
    public void TearDown() => tempDir.Dispose();

    [Test]
    public void Empty_should_contain_no_files() => Assert.IsEmpty(FilesObserver.Empty);

    [Test]
    public void List_files_in_the_folder() {
      var fileA = tempDir.CreateFile("A", "A.txt");
      Assert.That(FilesObserver.Empty.ExtendWith(tempDir.Path, "*.txt"), Contains.Item(fileA));
    }

    [Test]
    public void Do_not_list_filtered_files() {
      tempDir.CreateFile("A", "A.txt");
      Assert.IsEmpty(FilesObserver.Empty.ExtendWith(tempDir.Path, "*.cs"));
    }

    [Test]
    public void List_files_from_two_separate_subfolders() {
      var fileA = tempDir.CreateFile("A", "A.txt");
      var fileB = tempDir.CreateFile("B", "B.cs");
      Assert.That(FilesObserver.Empty
                               .ExtendWith(Path.Combine(tempDir.Path, "B"), "*.cs")
                               .ExtendWith(Path.Combine(tempDir.Path, "A"), "*.txt"),
                  Contains.Item(fileA).And.Contains(fileB));
    }
  }
}