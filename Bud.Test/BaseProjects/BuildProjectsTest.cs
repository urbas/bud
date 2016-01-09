using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bud.IO;
using Bud.V1;
using Moq;
using NUnit.Framework;
using static System.IO.Directory;
using static System.IO.Path;
using static Bud.V1.Api;
using static NUnit.Framework.Assert;

namespace Bud.BaseProjects {
  public class BuildProjectsTest {
    [Test]
    public void Set_the_projectDir()
      => AreEqual("bar", ProjectDir[BuildProject("bar", "Foo")]);

    [Test]
    public void Set_the_projectId()
      => AreEqual("Foo", ProjectId[BuildProject("bar", "Foo")]);

    [Test]
    public void Target_directory_is_within_the_project_directory() {
      var project = BuildProject("fooDir", "foo");
      AreEqual(Combine(ProjectDir[project], "target"), TargetDir[project]);
    }

    [Test]
    public void Dependencies_should_be_initially_empty()
      => IsEmpty(Dependencies[BuildProject("bar", "Foo")]);

    [Test]
    public void Sources_should_contain_files_from_added_directories() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A", "A.cs");
        var fileB = tempDir.CreateEmptyFile("B", "B.cs");
        var twoDirsProject = BuildProject(tempDir.Path, "foo")
          .AddSources("A")
          .AddSources("B");
        That(Sources[twoDirsProject].Take(1).Wait(),
                    Is.EquivalentTo(new[] {fileA, fileB}));
      }
    }

    [Test]
    public void Sources_should_not_include_files_in_the_target_folder() {
      using (var tempDir = new TemporaryDirectory()) {
        var project = BuildProject(tempDir.Path, "foo").AddSources(fileFilter: "*.cs");
        tempDir.CreateEmptyFile(TargetDir[project], "A.cs");
        var files = Sources[project].Take(1).Wait();
        IsEmpty(files);
      }
    }

    [Test]
    public void Input_should_initially_observe_a_single_empty_inout()
      => AreEqual(new[] {Enumerable.Empty<string>()},
                         Input[BuildProject("bar", "Foo")].ToList().Wait());

    [Test]
    public void Input_contains_the_added_file() {
      var buildProject = BuildProject("foo", "Foo")
        .Add(SourceIncludes, c => FilesObservatory[c].WatchFiles("foo/bar"));
      AreEqual(new[] {"foo/bar"},
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
      AreEqual(expectedOutputFiles, actualOutputFiles);
    }

    [Test]
    public void Source_processors_must_be_invoked_on_the_build_pipeline_thread() {
      int inputThreadId = 0;
      var fileProcessor = new ThreadIdRecordingInputProcessor();
      BuildProject("fooDir", "A")
        .Add(SourceIncludes, new FileWatcher(Enumerable.Empty<string>(), Observable.Create<string>(observer => {
          Task.Run(() => {
            inputThreadId = Thread.CurrentThread.ManagedThreadId;
            observer.OnNext("A.cs");
            observer.OnCompleted();
          });
          return new CompositeDisposable();
        })))
        .Add(SourceProcessors, fileProcessor)
        .Get(ProcessedSources).Wait();
      AreNotEqual(0, fileProcessor.InvocationThreadId);
      AreNotEqual(inputThreadId, fileProcessor.InvocationThreadId);
    }

    [Test]
    public void Default_input_contains_processed_sources() {
      var projects = BuildProject("bDir", "B")
        .Add(SourceIncludes, new FileWatcher("b"))
        .Add(SourceProcessors, new FooAppenderInputProcessor());
      AreEqual(new[] {"bfoo"},
                      projects.Get(Input).Wait());
    }

    [Test]
    public void Clean_deletes_non_empty_target_folders() {
      using (var tmpDir = new TemporaryDirectory()) {
        tmpDir.CreateEmptyFile("target", "foo.txt");
        tmpDir.CreateEmptyFile("target", "dir", "bar.txt");
        BuildProject(Combine(tmpDir.Path), "A").Get(Clean);
        IsFalse(Exists(Combine(tmpDir.Path, "target")));
      }
    }

    [Test]
    public void Clean_does_nothing_when_the_target_folder_does_not_exist() {
      using (var tmpDir = new TemporaryDirectory()) {
        BuildProject(Combine(tmpDir.Path), "A").Get(Clean);
        IsFalse(Exists(Combine(tmpDir.Path, "target")));
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