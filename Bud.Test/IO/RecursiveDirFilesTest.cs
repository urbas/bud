using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bud.IO {
  public class RecursiveDirFilesTest {
    private TemporaryDirectory tempDir;

    [SetUp]
    public void SetUp() => tempDir = new TemporaryDirectory();

    [TearDown]
    public void TearDown() => tempDir.Dispose();

    [Test]
    public void Empty_should_contain_no_files() => Assert.IsEmpty(Files.Empty);

    [Test]
    public void List_files_in_the_folder() {
      var fileA = tempDir.CreateEmptyFile("A", "A.txt");
      Assert.That(new RecursiveDirFiles(tempDir.Path, "*.txt"), Contains.Item(fileA));
    }

    [Test]
    public void Do_not_list_filtered_files() {
      tempDir.CreateEmptyFile("A", "A.txt");
      Assert.IsEmpty(new RecursiveDirFiles(tempDir.Path, "*.cs"));
    }

    [Test]
    public void List_files_from_two_separate_subfolders() {
      var fileA = tempDir.CreateEmptyFile("A", "A.txt");
      var fileB = tempDir.CreateEmptyFile("B", "B.cs");
      Assert.That(new RecursiveDirFiles(Path.Combine(tempDir.Path, "B"), "*.cs")
                    .ExtendWith(Path.Combine(tempDir.Path, "A"), "*.txt"),
                  Contains.Item(fileA).And.Contains(fileB));
    }

    [Test]
    public async void Observers_should_be_notified_on_subscription() {
      var fileA = tempDir.CreateEmptyFile("A", "A.txt");
      var recursiveDirFiles = new RecursiveDirFiles(tempDir.Path, "*.txt");
      Assert.That(await recursiveDirFiles.AsObservable().Take(1).ToTask(), Contains.Item(fileA));
    }

    [Test]
    public async void Adding_a_file_should_result_in_a_push_notification() {
      var fileCreationLatch = new CountdownEvent(1);

      var file = Task.Run(() => {
        fileCreationLatch.Wait();
        // TODO: Find a way to not wait here...
        Thread.Sleep(TimeSpan.FromMilliseconds(100));
        return tempDir.CreateEmptyFile("A", "A.txt");
      });

      var observableFiles = new RecursiveDirFiles(tempDir.Path, "*.txt").AsObservable().Take(2);
      var fileLists = await observableFiles.Select(SampleFilesAndLiftLatch(fileCreationLatch)).ToList().ToTask();

      Assert.IsEmpty(fileLists[0]);
      Assert.That(fileLists[1], Contains.Item(await file));
    }

    private static Func<IFiles, int, List<string>> SampleFilesAndLiftLatch(CountdownEvent latch) {
      return (files, index) => {
        var filesList = files.ToList();
        if (index < 1) {
          latch.Signal();
        }
        return filesList;
      };
    }
  }
}