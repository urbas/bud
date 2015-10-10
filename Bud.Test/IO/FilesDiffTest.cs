using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Moq;
using NUnit.Framework;

namespace Bud.IO {
  public class FilesDiffTest {
    private Mock<IFileTimestamps> fileTimestamps;
    private FilesDiff initialDiff;

    [SetUp]
    public void SetUp() {
      fileTimestamps = new Mock<IFileTimestamps>();
      fileTimestamps.Setup(self => self.GetTimestamp(It.IsAny<string>())).Returns(DateTime.FromFileTime(1));
      initialDiff = FilesDiff.Create(fileTimestamps.Object, new[] {"a"}, FilesDiff.Empty);
    }

    [Test]
    public void Initially_all_files_are_added() {
      Assert.AreEqual(new[] {"a"}, initialDiff.AddedFiles);
      Assert.IsEmpty(initialDiff.RemovedFiles);
      Assert.IsEmpty(initialDiff.ChangedFiles);
      Assert.AreEqual(new[] {"a"}, initialDiff.AllFiles);
    }

    [Test]
    public void List_newly_added_files() {
      var diff = FilesDiff.Create(fileTimestamps.Object, new[] { "a", "b" }, initialDiff);
      Assert.AreEqual(new[] {"b"}, diff.AddedFiles);
      Assert.IsEmpty(diff.RemovedFiles);
      Assert.IsEmpty(diff.ChangedFiles);
      Assert.AreEqual(new[] {"a", "b"}, diff.AllFiles);
    }

    [Test]
    public void List_removed_files() {
      var diff = FilesDiff.Create(fileTimestamps.Object, Enumerable.Empty<string>(), initialDiff);
      Assert.AreEqual(new[] {"a"}, diff.RemovedFiles);
      Assert.IsEmpty(diff.AddedFiles);
      Assert.IsEmpty(diff.ChangedFiles);
      Assert.IsEmpty(diff.AllFiles);
    }

    [Test]
    public void No_changes() {
      var diff = FilesDiff.Create(fileTimestamps.Object, new[] { "a" }, initialDiff);
      Assert.IsEmpty(diff.RemovedFiles);
      Assert.IsEmpty(diff.AddedFiles);
      Assert.IsEmpty(diff.ChangedFiles);
      Assert.AreEqual(new[] {"a"}, diff.AllFiles);
    }

    [Test]
    public void Changed_file() {
      fileTimestamps.Setup(self => self.GetTimestamp(It.IsAny<string>())).Returns(DateTime.FromFileTime(2));
      var diff = FilesDiff.Create(fileTimestamps.Object, ImmutableArray.Create("a"), initialDiff);
      Assert.IsEmpty(diff.RemovedFiles);
      Assert.IsEmpty(diff.AddedFiles);
      Assert.AreEqual(new[] {"a"}, diff.ChangedFiles);
      Assert.AreEqual(new[] {"a"}, diff.AllFiles);
    }
  }
}