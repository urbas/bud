using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Bud.IO {
  public class FilesObservatoryTest {
    private readonly EmptyFilesObservatory noFileChanges = new EmptyFilesObservatory();

    [Test]
    public void List_files_in_the_folder() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A", "A.txt");
        Assert.That(noFileChanges.ObserveDir(tempDir.Path, "*.txt", true).Resources,
                    Is.EquivalentTo(new[] {fileA}));
      }
    }

    [Test]
    public void Do_not_list_filtered_files() {
      using (var tempDir = new TemporaryDirectory()) {
        tempDir.CreateEmptyFile("A", "A.txt");
        Assert.IsEmpty(noFileChanges.ObserveDir(tempDir.Path, "*.cs", true).Resources);
      }
    }

    [Test]
    public void Do_not_list_in_subfolders() {
      using (var tempDir = new TemporaryDirectory()) {
        tempDir.CreateEmptyFile("A", "A.txt");
        Assert.IsEmpty(noFileChanges.ObserveDir(tempDir.Path, "*.txt", false).Resources);
      }
    }

    [Test]
    public void Throws_for_non_existing_folders() {
      using (var tempDir = new TemporaryDirectory()) {
        Assert.Throws<DirectoryNotFoundException>(() => { noFileChanges.ObserveDir(Path.Combine(tempDir.Path, "B"), "*.txt", true).Resources.ToList(); });
      }
    }

    [Test]
    public void Listing_individual_files_should_produce_the_first_observation() {
      var fileB = "foo";
      Assert.AreEqual(new[] {fileB},
                      noFileChanges.ObserveFiles(fileB).Resources);
    }
  }
}