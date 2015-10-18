using System.Collections.Generic;
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
      => Assert.That(noFileChanges.ObserveDir(tempDir.Path, "*.txt", true), Contains.Item(fileA));

    [Test]
    public void Do_not_list_filtered_files()
      => Assert.IsEmpty(noFileChanges.ObserveDir(tempDir.Path, "*.cs", true));

    [Test]
    public void Do_not_list_in_subfolders()
      => Assert.IsEmpty(noFileChanges.ObserveDir(tempDir.Path, "*.txt", false));

    [Test]
    public void Throws_for_non_existing_folders()
      => Assert.Throws<DirectoryNotFoundException>(() => noFileChanges.ObserveDir(Path.Combine(tempDir.Path, "B"), "*.txt", true).ToList());

    [Test]
    public void Listing_individual_files_should_produce_the_first_observation() {
      var fileB = Path.Combine(tempDir.Path, "B");
      Assert.That(noFileChanges.ObserveFiles(fileB).Watch().ToEnumerable().First(),
                  Is.EquivalentTo(new[] { Files.ToTimeHashedFile(fileB) }));
    }
  }
}