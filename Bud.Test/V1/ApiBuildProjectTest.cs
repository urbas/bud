using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bud.IO;
using Moq;
using NUnit.Framework;
using static System.IO.Path;
using static System.Linq.Enumerable;
using static Bud.IO.FileObservatories;
using static Bud.IO.Watched;
using static Bud.V1.Api;

namespace Bud.V1 {
  public class ApiBuildProjectTest {
    [Test]
    public void Set_the_projectDir()
      => Assert.AreEqual("bar", ProjectDir[BuildProject("bar", "Foo")]);

    [Test]
    public void Set_the_projectId()
      => Assert.AreEqual("Foo", ProjectId[BuildProject("bar", "Foo")]);

    [Test]
    public void Target_directory_is_within_the_project_directory() {
      var project = BuildProject("fooDir", "foo");
      Assert.AreEqual(Combine(ProjectDir[project], "target"), TargetDir[project]);
    }

    [Test]
    public void Dependencies_should_be_initially_empty()
      => Assert.IsEmpty(Dependencies[BuildProject("bar", "Foo")]);

    [Test]
    public void Sources_should_contain_files_from_added_directories() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A", "A.cs");
        var fileB = tempDir.CreateEmptyFile("B", "B.cs");
        var twoDirsProject = BuildProject(tempDir.Path, "foo")
          .AddSources("A")
          .AddSources("B");
        Assert.That(Sources[twoDirsProject].Take(1).Wait(),
                    Is.EquivalentTo(new[] {fileA, fileB}));
      }
    }

    [Test]
    public void Sources_should_not_include_files_in_the_target_folder() {
      using (var tempDir = new TemporaryDirectory()) {
        var project = BuildProject(tempDir.Path, "foo").AddSources(fileFilter: "*.cs");
        tempDir.CreateEmptyFile(TargetDir[project], "A.cs");
        var files = Sources[project].Take(1).Wait();
        Assert.IsEmpty(files);
      }
    }

    [Test]
    public void Input_should_initially_observe_a_single_empty_inout()
      => Assert.AreEqual(new[] {Empty<string>()},
                         Input[BuildProject("bar", "Foo")].ToList().Wait());

    [Test]
    public void Input_contains_the_added_file() {
      var buildProject = BuildProject("foo", "Foo")
        .Add(SourceIncludes, c => FilesObservatory[c].ObserveFiles("foo/bar"));
      Assert.AreEqual(new[] {"foo/bar"},
                      Input[buildProject].Take(1).Wait());
    }

    [Test]
    public void Source_processor_changes_source_input() {
      var fileProcessor = new Mock<IInputProcessor>(MockBehavior.Strict);
      var expectedOutputFiles = new[] {"foo"};
      fileProcessor.Setup(self => self.Process(It.IsAny<IObservable<IEnumerable<string>>>()))
                   .Returns(Observable.Return(expectedOutputFiles));
      var actualOutputFiles = BuildProject("FooDir", "Foo")
        .Add(SourceProcessors, fileProcessor.Object)
        .Get(ProcessedSources)
        .Wait();
      fileProcessor.VerifyAll();
      Assert.AreEqual(expectedOutputFiles, actualOutputFiles);
    }

    [Test]
    public void Source_processors_must_be_invoked_on_the_build_pipeline_thread() {
      int inputThreadId = 0;
      var fileProcessor = new ThreadIdRecordingInputProcessor();
      BuildProject("fooDir", "A")
        .Add(SourceIncludes, WatchFiles(Empty<string>(), Observable.Create<IEnumerable<string>>(observer => {
          Task.Run(() => {
            inputThreadId = Thread.CurrentThread.ManagedThreadId;
            observer.OnNext(Empty<string>());
            observer.OnCompleted();
          });
          return new CompositeDisposable();
        })))
        .Add(SourceProcessors, fileProcessor)
        .Get(ProcessedSources).Wait();
      Assert.AreNotEqual(0, fileProcessor.InvocationThreadId);
      Assert.AreNotEqual(inputThreadId, fileProcessor.InvocationThreadId);
    }

    [Test]
    public void Default_input_contains_processed_sources() {
      var projects = BuildProject("bDir", "B")
        .Add(SourceIncludes, UnchangingFiles("b"))
        .Add(SourceProcessors, new FooAppenderInputProcessor());
      Assert.AreEqual(new[] {"bfoo"},
                      projects.Get(Input).Wait());
    }

    [Test]
    public void Clean_deletes_non_empty_target_folders() {
      using (var tmpDir = new TemporaryDirectory()) {
        tmpDir.CreateEmptyFile("target", "foo.txt");
        tmpDir.CreateEmptyFile("target", "dir", "bar.txt");
        BuildProject(Combine(tmpDir.Path), "A").Get(Clean);
        Assert.IsFalse(Directory.Exists(Combine(tmpDir.Path, "target")));
      }
    }

    [Test]
    public void Clean_does_nothing_when_the_target_folder_does_not_exist() {
      using (var tmpDir = new TemporaryDirectory()) {
        BuildProject(Combine(tmpDir.Path), "A").Get(Clean);
        Assert.IsFalse(Directory.Exists(Combine(tmpDir.Path, "target")));
      }
    }

    private class FooAppenderInputProcessor : IInputProcessor {
      public IObservable<IEnumerable<string>> Process(IObservable<IEnumerable<string>> sources)
        => sources.Select(io => io.Select(file => file + "foo"));
    }

    public class ThreadIdRecordingInputProcessor : IInputProcessor {
      public int InvocationThreadId { get; private set; }

      public IObservable<IEnumerable<string>> Process(IObservable<IEnumerable<string>> sources) {
        InvocationThreadId = Thread.CurrentThread.ManagedThreadId;
        return sources;
      }
    }
  }
}