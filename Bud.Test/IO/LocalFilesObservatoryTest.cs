using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using static Bud.IO.LocalFilesObservatory;

namespace Bud.IO {
  public class LocalFilesObservatoryTest {
    [Test]
    public async void Adding_a_file_should_result_in_a_push_notification() {
      using (var tempDir = new TemporaryDirectory()) {
        var disposingBarrier = new CountdownEvent(1);
        var observedChanges = ObserveSingleFileChangeAsync(tempDir, disposingBarrier);
        var file = tempDir.CreateEmptyFile("A", "A.txt");
        Assert.AreEqual(file, await observedChanges);
        disposingBarrier.Wait();
      }
    }

    [Test]
    public async void Changing_a_file_should_result_in_a_push_notification() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A", "A.txt");
        var disposingBarrier = new CountdownEvent(1);
        var observedChanges = ObserveSingleFileChangeAsync(tempDir, disposingBarrier);
        File.WriteAllText(fileA, "foo");
        Assert.AreEqual(fileA, await observedChanges);
        disposingBarrier.Wait();
      }
    }

    [Test]
    public async void Removing_a_file_should_result_in_a_push_notification() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A", "A.txt");
        var disposingBarrier = new CountdownEvent(1);
        var observedChanges = ObserveSingleFileChangeAsync(tempDir, disposingBarrier);
        File.Delete(fileA);
        Assert.AreEqual(fileA, await observedChanges);
        disposingBarrier.Wait();
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

    [Test]
    [Timeout(2000)]
    public async void Merging_observers_should_produce_observations_when_only_one_produces_them() {
      using (var tempDir = new TemporaryDirectory()) {
        tempDir.CreateEmptyFile("A", "A.txt");
        var fileB = tempDir.CreateEmptyFile("B", "B.txt");
        var dirA = Path.Combine(tempDir.Path, "A");
        var dirB = Path.Combine(tempDir.Path, "B");
        var disposalBarrier = new CountdownEvent(2);
        var observations = ObserveMergedFileChanges(dirA, dirB, 6, disposalBarrier);
        File.WriteAllText(fileB, "moo");
        File.WriteAllText(fileB, "foo");
        File.WriteAllText(fileB, "boo");
        var actual = await observations;
        Assert.AreEqual(new[] {fileB, fileB, fileB, fileB, fileB, fileB},
                        actual);
        disposalBarrier.Wait();
      }
    }

    [Test]
    public void Observing_a_nonexisting_directory_should_fail() {
      using (var tempDir = new TemporaryDirectory()) {
        var dir = Path.Combine(tempDir.Path, "A");
        Assert.Throws<ArgumentException>(
          () => ObserveFileSystem(dir, "*", true).Wait());
      }
    }

    private static Task<string> ObserveSingleFileChangeAsync(TemporaryDirectory tempDir,
                                                             CountdownEvent disposingBarrier)
      => InvokeWithBarrierAsync(@event => Task.Run(() => {
        return ObserveFileSystem(tempDir.Path, "*.txt", true, () => @event.Signal(), () => disposingBarrier.Signal())
          .Take(1)
          .Wait();
      }), 1);

    private static Task<List<string>> ObserveFileChanges(TemporaryDirectory tempDir,
                                                         int expectedChangesCount,
                                                         Action disposedCallback)
      => InvokeWithBarrierAsync(@event => Task.Run(() => {
        return ObserveFileSystem(tempDir.Path, "*.txt", true, () => @event.Signal(), disposedCallback)
          .Take(expectedChangesCount)
          .ToEnumerable().ToList();
      }), 1);

    private static Task<List<string>> ObserveMergedFileChanges(string dirA,
                                                               string dirB,
                                                               int expectedChangesCount,
                                                               CountdownEvent disposalBarrier)
      => InvokeWithBarrierAsync(subscriptionBarrier => Task.Run(() => {
        var observerA = ObserveFileSystem(dirA, "*", true, () => subscriptionBarrier.Signal(), () => disposalBarrier.Signal());
        var observerB = ObserveFileSystem(dirB, "*", true, () => subscriptionBarrier.Signal(), () => disposalBarrier.Signal());
        return observerA.Merge(observerB)
                        .ObserveOn(new EventLoopScheduler())
                        .Take(expectedChangesCount)
                        .ToEnumerable().ToList();
      }), 2);

    private static T InvokeWithBarrierAsync<T>(Func<CountdownEvent, T> watchFunc, int barrierCount) {
      var barrier = new CountdownEvent(barrierCount);
      var observedChanges = watchFunc(barrier);
      barrier.Wait();
      return observedChanges;
    }
  }
}