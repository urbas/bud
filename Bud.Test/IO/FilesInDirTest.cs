using System.IO;
using System.Linq;
using System.Reactive.Linq;
using NUnit.Framework;

namespace Bud.IO {
  public class FilesInDirTest {
    private TemporaryDirectory tempDir;
    private readonly EmptyFilesObservatory noFileChanges = new EmptyFilesObservatory();

    [SetUp]
    public void SetUp() => tempDir = new TemporaryDirectory();

    [TearDown]
    public void TearDown() => tempDir.Dispose();

    [Test]
    public void Empty_should_contain_no_files() => Assert.IsEmpty(Files.Empty.Enumerate());

    [Test]
    public void List_files_in_the_folder() {
      var fileA = tempDir.CreateEmptyFile("A", "A.txt");
      Assert.That(new FilesInDir(noFileChanges, tempDir.Path, "*.txt").Enumerate(), Contains.Item(fileA));
    }

    [Test]
    public void Do_not_list_filtered_files() {
      tempDir.CreateEmptyFile("A", "A.txt");
      Assert.IsEmpty(new FilesInDir(noFileChanges, tempDir.Path, "*.cs").Enumerate());
    }

    [Test]
    public void List_files_from_two_separate_subfolders() {
      var fileA = tempDir.CreateEmptyFile("A", "A.txt");
      var fileB = tempDir.CreateEmptyFile("B", "B.cs");
      Assert.That(new FilesInDir(noFileChanges, Path.Combine(tempDir.Path, "B"), "*.cs")
                    .ExtendWith(new FilesInDir(noFileChanges, Path.Combine(tempDir.Path, "A"), "*.txt"))
                    .Enumerate(),
                  Contains.Item(fileA).And.Contains(fileB));
    }

    [Test]
    public void Observers_should_be_notified_directly_on_subscription() {
      var fileA = tempDir.CreateEmptyFile("A", "A.txt");
      var recursiveDirFiles = new FilesInDir(noFileChanges, tempDir.Path, "*.txt");
      Assert.That(recursiveDirFiles.Watch().ToEnumerable().First().Enumerate(), Contains.Item(fileA));
    }
  }
}