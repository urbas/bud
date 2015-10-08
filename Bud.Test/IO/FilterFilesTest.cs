using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using NUnit.Framework;

namespace Bud.IO {
  public class FilterFilesTest {
    private ListedFiles someFiles;
    private IObservable<FileSystemEventArgs> filesObservable;
    private ImmutableArray<FileSystemEventArgs> fileSystemEvents;
    private RenamedEventArgs renamedEventArgs;

    [SetUp]
    public void SetUp() {
      renamedEventArgs = new RenamedEventArgs(WatcherChangeTypes.Renamed, "goo", "f", "h");
      fileSystemEvents = ImmutableArray.Create(new FileSystemEventArgs(WatcherChangeTypes.Created, "moo", "g"),
                                               new FileSystemEventArgs(WatcherChangeTypes.Deleted, "", "a"),
                                               new FileSystemEventArgs(WatcherChangeTypes.Changed, "foo", "b"),
                                               renamedEventArgs);
      filesObservable = fileSystemEvents.ToObservable();
      someFiles = new ListedFiles(filesObservable, ImmutableArray.Create("a", "foo/b", "bar/c"));
    }

    [Test]
    public void Exclude_everything() {
      Assert.IsEmpty(new FilterFiles(someFiles, path => false));
    }

    [Test]
    public void Exclude_nothing() {
      Assert.That(new FilterFiles(someFiles, path => true), Is.EquivalentTo(someFiles));
    }

    [Test]
    public void Exclude_some_files() {
      Assert.That(new FilterFiles(someFiles, path => path.Contains("c")), Is.EquivalentTo(ImmutableArray.Create("bar/c")));
    }

    [Test]
    public void Include_renames() {
      var observations = new FilterFiles(someFiles, path => path.EndsWith("h")).AsObservable().ToEnumerable();
      Assert.AreEqual(ImmutableArray.Create(renamedEventArgs), observations.Select(update => update.FileSystemEventArgs).Skip(1));
    }

    [Test]
    public void Exclude_all_but_first_observation() {
      var observations = new FilterFiles(someFiles, path => false).AsObservable().ToEnumerable();
      Assert.IsEmpty(observations.Select(update => update.FileSystemEventArgs).Skip(1));
    }

    [Test]
    public void Exclude_no_observations() {
      var observations = new FilterFiles(someFiles, path => true).AsObservable().ToEnumerable();
      Assert.AreEqual(fileSystemEvents, observations.Select(update => update.FileSystemEventArgs).Skip(1));
    }

    [Test]
    public void Observe_filtered_files_only() {
      var filesUpdate = new FilterFiles(someFiles, path => path.Contains("a")).AsObservable().ToEnumerable().First();
      Assert.AreEqual(new [] {"a", "bar/c"}, filesUpdate);
    }
  }
}