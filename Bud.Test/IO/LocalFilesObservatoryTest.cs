using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bud.IO {
  public class LocalFilesObservatoryTest {
    [Test]
    public async void Adding_a_file_should_result_in_a_push_notification() {
      using (var tempDir = new TemporaryDirectory()) {
        bool wasDisposed = false;
        var observedChanges = ObserveSingleFileChangeAsync(tempDir, () => wasDisposed = true);
        var file = tempDir.CreateEmptyFile("A", "A.txt");
        Assert.AreEqual(file, await observedChanges);
        Assert.IsTrue(wasDisposed);
      }
    }

    [Test]
    public async void Changing_a_file_should_result_in_a_push_notification() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A", "A.txt");
        bool wasDisposed = false;
        var observedChanges = ObserveSingleFileChangeAsync(tempDir, () => wasDisposed = true);
        File.WriteAllText(fileA, "foo");
        Assert.AreEqual(fileA, await observedChanges);
        Assert.IsTrue(wasDisposed);
      }
    }

    [Test]
    public async void Removing_a_file_should_result_in_a_push_notification() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A", "A.txt");
        bool wasDisposed = false;
        var observedChanges = ObserveSingleFileChangeAsync(tempDir, () => wasDisposed = true);
        File.Delete(fileA);
        Assert.AreEqual(fileA, await observedChanges);
        Assert.IsTrue(wasDisposed);
      }
    }

    [Test]
    public async void Moving_a_file_should_result_in_two_push_notifications() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A", "A.txt");
        var fileB = Path.Combine(tempDir.Path, "B.txt");
        bool wasDisposed = false;
        var changesTask = ObserveFileChanges(tempDir, 2, () => wasDisposed = true);
        File.Move(fileA, fileB);
        var changes = await changesTask;
        Assert.AreEqual(fileA, changes[0]);
        Assert.AreEqual(fileB, changes[1]);
        Assert.IsTrue(wasDisposed);
      }
    }

    [Test]
    public async void Renaming_a_file_should_result_in_two_push_notifications() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A.txt");
        var fileB = Path.Combine(tempDir.Path, "B.txt");
        bool wasDisposed = false;
        var changesTask = ObserveFileChanges(tempDir, 2, () => wasDisposed = true);
        File.Move(fileA, fileB);
        var changes = await changesTask;
        Assert.AreEqual(fileA, changes[0]);
        Assert.AreEqual(fileB, changes[1]);
        Assert.IsTrue(wasDisposed);
      }
    }

    [Test]
    public async void Changing_multiple_files_must_produce_multiple_observations() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A.txt");
        var fileB = tempDir.CreateEmptyFile("B", "B.txt");
        bool wasDisposed = false;
        var changesTask = ObserveFileChanges(tempDir, 6, () => wasDisposed = true);
        File.WriteAllText(fileA, "foo");
        File.WriteAllText(fileB, "moo");
        File.WriteAllText(fileA, "goo");
        var changes = await changesTask;
        Assert.AreEqual(new[] {fileA, fileA, fileB, fileB, fileA, fileA},
                        changes);
        Assert.IsTrue(wasDisposed);
      }
    }

    private static Task<string> ObserveSingleFileChangeAsync(TemporaryDirectory tempDir,
                                                             Action disposedCallback)
      => InvokeWithBarrierAsync(@event => Task.Run(() => {
        return LocalFilesObservatory
          .ObserveFileSystem(tempDir.Path, "*.txt", true, () => @event.Signal(), disposedCallback)
          .Take(1)
          .Wait();
      }));

    private static Task<List<string>> ObserveFileChanges(TemporaryDirectory tempDir,
                                                         int expectedChangesCount,
                                                         Action disposedCallback)
      => InvokeWithBarrierAsync(@event => Task.Run(() => {
        return LocalFilesObservatory
          .ObserveFileSystem(tempDir.Path, "*.txt", true, () => @event.Signal(), disposedCallback)
          .Take(expectedChangesCount)
          .ToEnumerable().ToList();
      }));

    private static T InvokeWithBarrierAsync<T>(Func<CountdownEvent, T> watchFunc) {
      var barrier = new CountdownEvent(1);
      var observedChanges = watchFunc(barrier);
      barrier.Wait();
      return observedChanges;
    }
  }
}