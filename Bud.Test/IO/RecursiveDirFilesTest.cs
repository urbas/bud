using System.IO;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using NUnit.Framework;

namespace Bud.IO {
  public class RecursiveDirFilesTest {
    private TemporaryDirectory tempDir;

    [SetUp]
    public void SetUp() => tempDir = new TemporaryDirectory();

    [TearDown]
    public void TearDown() => tempDir.Dispose();

    [Test]
    public void Empty_should_contain_no_files() => Assert.IsEmpty(Files.Empty);

    [Test]
    public void List_files_in_the_folder() {
      var fileA = tempDir.CreateEmptyFile("A", "A.txt");
      Assert.That(new RecursiveDirFiles(new FileSystemObserverFactory(), tempDir.Path, "*.txt"), Contains.Item(fileA));
    }

    [Test]
    public void Do_not_list_filtered_files() {
      tempDir.CreateEmptyFile("A", "A.txt");
      Assert.IsEmpty(new RecursiveDirFiles(new FileSystemObserverFactory(), tempDir.Path, "*.cs"));
    }

    [Test]
    public void List_files_from_two_separate_subfolders() {
      var fileA = tempDir.CreateEmptyFile("A", "A.txt");
      var fileB = tempDir.CreateEmptyFile("B", "B.cs");
      Assert.That(new RecursiveDirFiles(new FileSystemObserverFactory(), Path.Combine(tempDir.Path, "B"), "*.cs")
                    .ExtendWith(new FileSystemObserverFactory(), Path.Combine(tempDir.Path, "A"), "*.txt"),
                  Contains.Item(fileA).And.Contains(fileB));
    }

    [Test]
    public async void Observers_should_be_notified_on_subscription() {
      var fileA = tempDir.CreateEmptyFile("A", "A.txt");
      var recursiveDirFiles = new RecursiveDirFiles(new FileSystemObserverFactory(), tempDir.Path, "*.txt");
      Assert.That(await recursiveDirFiles.AsObservable().Take(1).ToTask(), Contains.Item(fileA));
    }
  }
}