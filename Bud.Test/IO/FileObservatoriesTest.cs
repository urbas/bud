using System.IO;
using System.Linq;
using Bud.TempDir;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Bud.IO {
  public class FileObservatoriesTest {
    private readonly NoOpFilesObservatory noFileChanges = new NoOpFilesObservatory();

    [Test]
    public void List_files_in_the_folder() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A", "A.txt");
        That(noFileChanges.WatchDir(tempDir.Path, "*.txt", true).Files,
             Is.EquivalentTo(new[] {fileA}));
      }
    }

    [Test]
    public void Do_not_list_filtered_files() {
      using (var tempDir = new TemporaryDirectory()) {
        tempDir.CreateEmptyFile("A", "A.txt");
        IsEmpty(noFileChanges.WatchDir(tempDir.Path, "*.cs", true).Files);
      }
    }

    [Test]
    public void Do_not_list_in_subfolders() {
      using (var tempDir = new TemporaryDirectory()) {
        tempDir.CreateEmptyFile("A", "A.txt");
        IsEmpty(noFileChanges.WatchDir(tempDir.Path, "*.txt", false).Files);
      }
    }

    [Test]
    public void Throws_for_non_existing_folders() {
      using (var tempDir = new TemporaryDirectory()) {
        Throws<DirectoryNotFoundException>(() => {
          noFileChanges.WatchDir(Path.Combine(tempDir.Path, "B"), "*.txt", true).Files.ToList();
        });
      }
    }

    [Test]
    public void Listing_individual_files_should_produce_the_first_observation() {
      var fileB = "foo";
      AreEqual(new[] {fileB},
               noFileChanges.WatchFiles(fileB).Files);
    }
  }
}