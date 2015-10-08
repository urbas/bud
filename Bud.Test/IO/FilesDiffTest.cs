using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Moq;
using NUnit.Framework;

namespace Bud.IO {
  public class FilesDiffTest {
    [Test]
    public void Initially_all_files_are_added() {
      var filesUpdates = ImmutableArray.Create(new ListedFiles("a", "b")).ToObservable();
      var diff = FilesDiff.DoDiffing(filesUpdates).ToEnumerable().First();
      Assert.AreEqual(new[] {"a", "b"}, diff.AddedFiles);
      Assert.IsEmpty(diff.RemovedFiles);
      Assert.IsEmpty(diff.ChangedFiles);
      Assert.AreEqual(new[] {"a", "b"}, diff.AllFiles);
    }

    [Test]
    public void List_newly_added_files() {
      var filesUpdates = ImmutableArray.Create(new ListedFiles("a"), new ListedFiles("a", "b")).ToObservable();
      var diff = FilesDiff.DoDiffing(filesUpdates).ToEnumerable().Last();
      Assert.AreEqual(new[] {"b"}, diff.AddedFiles);
      Assert.IsEmpty(diff.RemovedFiles);
      Assert.IsEmpty(diff.ChangedFiles);
      Assert.AreEqual(new[] {"a", "b"}, diff.AllFiles);
    }

    [Test]
    public void List_removed_files() {
      var filesUpdates = ImmutableArray.Create(new ListedFiles("a", "b"), new ListedFiles("a")).ToObservable();
      var diff = FilesDiff.DoDiffing(filesUpdates).ToEnumerable().Last();
      Assert.AreEqual(new[] {"b"}, diff.RemovedFiles);
      Assert.IsEmpty(diff.AddedFiles);
      Assert.IsEmpty(diff.ChangedFiles);
      Assert.AreEqual(new[] {"a"}, diff.AllFiles);
    }

    [Test]
    public void No_changes() {
      var filesUpdates = ImmutableArray.Create(new ListedFiles("a"), new ListedFiles("a")).ToObservable();
      var diff = FilesDiff.DoDiffing(filesUpdates).ToEnumerable().Last();
      Assert.IsEmpty(diff.RemovedFiles);
      Assert.IsEmpty(diff.AddedFiles);
      Assert.IsEmpty(diff.ChangedFiles);
      Assert.AreEqual(new[] {"a"}, diff.AllFiles);
    }

    [Test]
    public void Changed_file() {
      var fileTimestamps = new Mock<IFileTimestamps>();
      long fileTimestamp = 0L;
      var filesUpdates = ImmutableArray.Create(new ListedFiles("a"), new ListedFiles("a"))
                                       .ToObservable()
                                       .Do(update => fileTimestamps.Setup(self => self.GetTimestamp(It.Is<string>(s => "a".Equals(s)))).Returns(DateTime.FromFileTime(++fileTimestamp)));
      var diff = FilesDiff.DoDiffing(filesUpdates, fileTimestamps.Object).ToEnumerable().Last();
      Assert.IsEmpty(diff.RemovedFiles);
      Assert.IsEmpty(diff.AddedFiles);
      Assert.AreEqual(new[] {"a"}, diff.ChangedFiles);
      Assert.AreEqual(new[] {"a"}, diff.AllFiles);
    }
  }
}