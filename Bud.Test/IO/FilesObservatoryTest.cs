using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public void Empty_observes_empty_files_only_once()
      => Assert.That(FilesObservatory.Empty.ToEnumerable(), Is.EquivalentTo(ImmutableArray.Create(Enumerable.Empty<string>())));

    [Test]
    public void Empty_observable_is_reusable()
    {
      Empty_observes_empty_files_only_once();
      Empty_observes_empty_files_only_once();
    }

    [Test]
    public void List_files_in_the_folder()
      => Assert.That(noFileChanges.ObserveFiles(tempDir.Path, "*.txt", true).ToEnumerable().First(), Contains.Item(fileA));

    [Test]
    public void Do_not_list_filtered_files()
      => Assert.IsEmpty(noFileChanges.ObserveFiles(tempDir.Path, "*.cs", true).ToEnumerable().First());

    [Test]
    public void Do_not_list_in_subfolders()
      => Assert.IsEmpty(noFileChanges.ObserveFiles(tempDir.Path, "*.txt", false).ToEnumerable().First());

    [Test]
    public void Throws_for_non_existing_folders()
      => Assert.Throws<DirectoryNotFoundException>(() => noFileChanges.ObserveFiles(Path.Combine(tempDir.Path, "B"), "*.txt", true).ToEnumerable().First());

    [Test]
    public void Observation_does_not_enumerate_the_files_if_nobody_pulls() {
      var fileEnumerator = new Mock<Func<string, string, bool, IEnumerable<string>>>(MockBehavior.Strict);
      noFileChanges.ObserveFiles(fileEnumerator.Object, Path.Combine(tempDir.Path, "B"), "*.txt", true).ToEnumerable();
    }

    [Test]
    public void Listing_individual_files_should_produce_the_first_observation()
      => Assert.That(noFileChanges.ObserveFileList(Path.Combine(tempDir.Path, "B")).ToEnumerable().First(),
                     Is.EquivalentTo(ImmutableArray.Create(new[] {Path.Combine(tempDir.Path, "B")})));

    [Test]
    public void Joining_empties_should_produce_a_single_update()
      => Assert.That(FilesObservatory.Join(FilesObservatory.Empty, FilesObservatory.Empty).ToEnumerable(),
                     Is.EquivalentTo(ImmutableArray.Create(Enumerable.Empty<string>())));

    [Test]
    public void Joining_an_empty_with_a_file_listing_should_produce_a_single_update()
      => Assert.That(FilesObservatory.Join(FilesObservatory.Empty, noFileChanges.ObserveFileList("a")).ToEnumerable(),
                     Is.EquivalentTo(ImmutableArray.Create(ImmutableArray.Create("a"))));

    [Test]
    public void Filter_can_exclude_everything()
      => Assert.That(FilesObservatory.Filter(noFileChanges.ObserveFileList("a"), s => false).ToEnumerable().First(),
                     Is.Empty);

    [Test]
    public void Filter_can_exclude_nothing()
      => Assert.That(FilesObservatory.Filter(noFileChanges.ObserveFileList("a"), s => true).ToEnumerable().First(),
                     Is.EquivalentTo(ImmutableArray.Create("a")));

    [Test]
    public void Filter_can_exclude_something()
      => Assert.That(FilesObservatory.Filter(noFileChanges.ObserveFileList("a", "b"), s => s == "b").ToEnumerable().First(),
                     Is.EquivalentTo(ImmutableArray.Create("b")));
  }
}