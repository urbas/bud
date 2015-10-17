using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Moq;
using NUnit.Framework;

namespace Bud.IO {
  public class FilesObservatoryTest {
    private TemporaryDirectory tempDir;
    private readonly EmptyFilesObservatory noFileChanges = new EmptyFilesObservatory();
    private string fileA;

    [SetUp]
    public void SetUp() {
      tempDir = new TemporaryDirectory();
      fileA = tempDir.CreateEmptyFile("A", "A.txt");
    }

    [TearDown]
    public void TearDown() => tempDir.Dispose();

    [Test]
    public void List_files_in_the_folder()
      => Assert.That(noFileChanges.ObserveDir(tempDir.Path, "*.txt", true).ToEnumerable().First(), Contains.Item(fileA));

    [Test]
    public void Do_not_list_filtered_files()
      => Assert.IsEmpty(noFileChanges.ObserveDir(tempDir.Path, "*.cs", true).ToEnumerable().First());

    [Test]
    public void Do_not_list_in_subfolders()
      => Assert.IsEmpty(noFileChanges.ObserveDir(tempDir.Path, "*.txt", false).ToEnumerable().First());

    [Test]
    public void Throws_for_non_existing_folders()
      => Assert.Throws<DirectoryNotFoundException>(() => noFileChanges.ObserveDir(Path.Combine(tempDir.Path, "B"), "*.txt", true).ToEnumerable().First());

    [Test]
    public void Observation_does_not_enumerate_the_files_if_nobody_pulls() {
      var fileEnumerator = new Mock<FileFinder>(MockBehavior.Strict);
      noFileChanges.ObserveDir(fileEnumerator.Object, Path.Combine(tempDir.Path, "B"), "*.txt", true).ToEnumerable();
    }

    [Test]
    public void Listing_individual_files_should_produce_the_first_observation() {
      var fileB = Path.Combine(tempDir.Path, "B");
      Assert.That(noFileChanges.ObserveFiles(fileB).ToEnumerable().First(),
                  Is.EquivalentTo(new[] { Files.ToTimeHashedFile(fileB) }));
    }
  }
}