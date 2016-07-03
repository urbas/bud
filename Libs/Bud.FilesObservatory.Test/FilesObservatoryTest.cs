using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using static Bud.FilesObservatory;

namespace Bud {
  [Category("AppVeyorIgnore")]
  public class FilesObservatoryTest {
    [Test]
    public async Task Adding_a_file_should_result_in_a_push_notification() {
      using (var dir = new TmpDir()) {
        var disposingBarrier = new CountdownEvent(1);
        var observedChanges = ObserveSingleFileChangeAsync(dir, disposingBarrier);
        var file = dir.CreateEmptyFile("A", "A.txt");
        Assert.AreEqual(file, await observedChanges);
        disposingBarrier.Wait();
      }
    }

    [Test]
    public async Task Changing_a_file_should_result_in_a_push_notification() {
      using (var dir = new TmpDir()) {
        var fileA = dir.CreateEmptyFile("A", "A.txt");
        var disposingBarrier = new CountdownEvent(1);
        var observedChanges = ObserveSingleFileChangeAsync(dir, disposingBarrier);
        File.WriteAllText(fileA, "foo");
        Assert.AreEqual(fileA, await observedChanges);
        disposingBarrier.Wait();
      }
    }

    [Test]
    public async Task Removing_a_file_should_result_in_a_push_notification() {
      using (var dir = new TmpDir()) {
        var fileA = dir.CreateEmptyFile("A", "A.txt");
        var disposingBarrier = new CountdownEvent(1);
        var observedChanges = ObserveSingleFileChangeAsync(dir, disposingBarrier);
        File.Delete(fileA);
        Assert.AreEqual(fileA, await observedChanges);
        disposingBarrier.Wait();
      }
    }

    [Test]
    public async Task Moving_a_file_should_result_in_two_push_notifications() {
      using (var dir = new TmpDir()) {
        var fileA = dir.CreateEmptyFile("A", "A.txt");
        var fileB = Path.Combine(dir.Path, "B.txt");
        bool wasDisposed = false;
        var changesTask = ObserveFileChanges(dir, 2, () => wasDisposed = true);
        File.Move(fileA, fileB);
        var changes = await changesTask;
        Assert.AreEqual(fileA, changes[0]);
        Assert.AreEqual(fileB, changes[1]);
        Assert.IsTrue(wasDisposed);
      }
    }

    [Test]
    public async Task Renaming_a_file_should_result_in_two_push_notifications() {
      using (var dir = new TmpDir()) {
        var fileA = dir.CreateEmptyFile("A.txt");
        var fileB = Path.Combine(dir.Path, "B.txt");
        bool wasDisposed = false;
        var changesTask = ObserveFileChanges(dir, 2, () => wasDisposed = true);
        File.Move(fileA, fileB);
        var changes = await changesTask;
        Assert.AreEqual(fileA, changes[0]);
        Assert.AreEqual(fileB, changes[1]);
        Assert.IsTrue(wasDisposed);
      }
    }

    [Test]
    public async Task Changing_multiple_files_must_produce_multiple_observations() {
      using (var dir = new TmpDir()) {
        var fileA = dir.CreateEmptyFile("A.txt");
        var fileB = dir.CreateEmptyFile("B", "B.txt");
        bool wasDisposed = false;
        var changesTask = ObserveFileChanges(dir, 6, () => wasDisposed = true);
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
    public async Task Merging_observers_should_produce_observations_when_only_one_produces_them() {
      using (var tempDir = new TmpDir()) {
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
      using (var dir = new TmpDir()) {
        var dirA = Path.Combine(dir.Path, "A");
        Assert.Throws<ArgumentException>(
          () => ObserveFileSystem(dirA, "*", true).Wait());
      }
    }

    [Test]
    public void Two_concurrent_subscribers_should_cause_only_one_subscription() {
      using (var dir = new TmpDir()) {
        int subscriptionCount = 0;
        var fileA = dir.CreateEmptyFile("A", "A.txt");
        var disposingBarrier = new CountdownEvent(1);
        // NOTE: Two events are generated per every change to a file. This is why we have
        // a countdown of two here.
        var observationACountdown = new CountdownEvent(2);
        var observationBCountdown = new CountdownEvent(2);
        TaskTestUtils.InvokeAndWait(
          waitCountdown => {
            var observable = ObserveFileSystem(dir.Path,
                                               fileFilter: "*.txt",
                                               includeSubdirectories: true,
                                               subscribedCallback: () => {
                                                 waitCountdown.Signal();
                                                 ++subscriptionCount;
                                               },
                                               disposedCallback: () => disposingBarrier.Signal());
            Task.Run(() => observable.Take(2)
                                     .Do(_ => observationACountdown.Signal())
                                     .Wait());
            Task.Run(() => observable.Take(2)
                                     .Do(_ => observationBCountdown.Signal())
                                     .Wait());
          },
          waitCountdown: 1);
        File.WriteAllText(fileA, "foo");
        observationACountdown.Wait();
        observationBCountdown.Wait();
        disposingBarrier.Wait();
        Assert.AreEqual(1, subscriptionCount);
      }
    }

    [Test]
    public void Observations_resume_after_first_disposal() {
      using (var dir = new TmpDir()) {
        var fileA = dir.CreateEmptyFile("A", "A.txt");
        var dispose1Barrier = new CountdownEvent(1);
        var dispose2Barrier = new CountdownEvent(2);
        var subscription2Barrier = new CountdownEvent(2);
        var fileObservatory = TaskTestUtils.InvokeAndWait(
          waitCountdown => {
            var observable = ObserveFileSystem(dir.Path,
                                               fileFilter: "*.txt",
                                               includeSubdirectories: true,
                                               subscribedCallback: () => {
                                                 if (!waitCountdown.IsSet) {
                                                   waitCountdown.Signal();
                                                 }
                                                 subscription2Barrier.Signal();
                                               },
                                               disposedCallback: () => {
                                                 if (!dispose1Barrier.IsSet) {
                                                   dispose1Barrier.Signal();
                                                 }
                                                 dispose2Barrier.Signal();
                                               });
            Task.Run(() => observable.Take(2).Wait());
            return observable;
          },
          waitCountdown: 1);
        File.WriteAllText(fileA, "foo");
        dispose1Barrier.Wait();
        var observationCountdown = new CountdownEvent(2);
        Task.Run(() => fileObservatory.Take(2).Do(_ => observationCountdown.Signal()).Wait());
        subscription2Barrier.Wait();
        File.WriteAllText(fileA, "bar");
        observationCountdown.Wait();
        dispose2Barrier.Wait();
      }
    }

    private static Task<string> ObserveSingleFileChangeAsync(TmpDir tempDir,
                                                             CountdownEvent disposingBarrier)
      => TaskTestUtils.InvokeAndWait(
        waitCountdown => Task.Run(() => {
          return ObserveFileSystem(tempDir.Path,
                                   fileFilter: "*.txt",
                                   includeSubdirectories: true,
                                   subscribedCallback: () => waitCountdown.Signal(),
                                   disposedCallback: () => disposingBarrier.Signal()).Take(1)
            .Wait();
        }),
        waitCountdown: 1);

    private static Task<List<string>> ObserveFileChanges(TmpDir tempDir,
                                                         int expectedChangesCount,
                                                         Action disposedCallback)
      => TaskTestUtils.InvokeAndWait(
        waitCountdown => Task.Run(
          () => ObserveFileSystem(tempDir.Path,
                                  fileFilter: "*.txt",
                                  includeSubdirectories: true,
                                  subscribedCallback: () => waitCountdown.Signal(),
                                  disposedCallback: disposedCallback).Take(expectedChangesCount)
                  .ToEnumerable().ToList()),
        waitCountdown: 1);

    private static Task<List<string>> ObserveMergedFileChanges(string dirA,
                                                               string dirB,
                                                               int expectedChangesCount,
                                                               CountdownEvent disposalBarrier)
      => TaskTestUtils.InvokeAndWait(waitCountdown => Task.Run(() => {
        var observerA = ObserveFileSystem(dirA, "*", true, () => waitCountdown.Signal(), () => disposalBarrier.Signal());
        var observerB = ObserveFileSystem(dirB, "*", true, () => waitCountdown.Signal(), () => disposalBarrier.Signal());
        return observerA.Merge(observerB).ObserveOn(new EventLoopScheduler())
                        .Take(expectedChangesCount)
                        .ToEnumerable().ToList();
      }), waitCountdown: 2);
  }
}