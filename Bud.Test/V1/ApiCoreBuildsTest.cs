using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bud.IO;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using static System.IO.Path;
using static System.Linq.Enumerable;
using static Bud.IO.InOutFile;
using static Bud.IO.Watched;
using static Bud.V1.Api;

namespace Bud.V1 {
  public class ApiCoreBuildsTest {
    [Test]
    public void Set_the_projectDir()
      => Assert.AreEqual("bar", ProjectDir[BuildProject("bar", "Foo")]);

    [Test]
    public void Set_the_projectId()
      => Assert.AreEqual("Foo", ProjectId[BuildProject("bar", "Foo")]);

    [Test]
    public void Sources_should_be_initially_empty()
      => Assert.IsEmpty(Sources[BuildProject("bar", "Foo")].Take(1).Wait());

    [Test]
    public void Dependencies_should_be_initially_empty()
      => Assert.IsEmpty(Dependencies[BuildProject("bar", "Foo")]);

    [Test]
    public void Input_should_initially_observe_a_single_empty_inout()
      => Assert.AreEqual(new[] {InOut.Empty}, Input[BuildProject("bar", "Foo")].ToList().Wait());

    [Test]
    public void Multiple_source_directories() {
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
    public void Target_directory_is_within_the_project_directory() {
      var project = BuildProject("fooDir", "foo");
      Assert.AreEqual(Combine(ProjectDir[project], "target"), TargetDir[project]);
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
    public void Source_processor_changes_source_input() {
      var fileProcessor = new Mock<IInputProcessor>(MockBehavior.Strict);
      var expectedOutputFiles = new InOut(ToInOutFile("foo"));
      fileProcessor.Setup(self => self.Process(It.IsAny<IObservable<InOut>>()))
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
        .Add(SourceIncludes, Watch(Empty<string>(), Observable.Create<string>(observer => {
          Task.Run(() => {
            inputThreadId = Thread.CurrentThread.ManagedThreadId;
            observer.OnNext("foo");
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
    public void Default_build_forwards_the_output_from_dependencies() {
      var projects = Projects(BuildProject("aDir", "A")
                             .Add(SourceIncludes, Watch(new[] {"a"})),
                           BuildProject("bDir", "B")
                             .Add(SourceIncludes, Watch(new[] {"b"}))
                             .Add(Dependencies, "../A"));
      Assert.AreEqual(new InOut(ToInOutFile("a"), ToInOutFile("b")),
                      projects.Get("B" / Output).Wait());
    }

    [Test]
    public void Input_reobserved_when_dependencies_change() {
      var testScheduler = new TestScheduler();
      var projects = Projects(BuildProject("aDir", "A")
                             .SetValue(BuildPipelineScheduler, testScheduler)
                             .Add(SourceIncludes, Watch(Empty<string>(), Observable.Return("foo").Delay(TimeSpan.FromSeconds(1), testScheduler)
                                                                                   .Concat(Observable.Return("bar").Delay(TimeSpan.FromSeconds(1), testScheduler)))),
                           BuildProject("bDir", "B")
                             .SetValue(BuildPipelineScheduler, testScheduler)
                             .Add(Dependencies, "../A"));
      var output = projects.Get("B" / Output).GetEnumerator();
      testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
      Assert.IsTrue(output.MoveNext());
      Assert.IsTrue(output.MoveNext());
      Assert.IsTrue(output.MoveNext());
    }

    [Test]
    public void Default_build_processes_own_sources_before_output() {
      var projects = Projects(BuildProject("aDir", "A")
                             .Add(SourceIncludes, Watch(new[] {"a"}))
                             .Add(SourceProcessors, new FooAppenderInputProcessor()),
                           BuildProject("bDir", "B")
                             .Add(SourceIncludes, Watch(new[] {"b"}))
                             .Add(SourceProcessors, new FooAppenderInputProcessor())
                             .Add(Dependencies, "../A"));
      Assert.AreEqual(new InOut(ToInOutFile("afoo"), ToInOutFile("bfoo")),
                      projects.Get("B" / Output).Wait());
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
      public IObservable<InOut> Process(IObservable<InOut> sources)
        => sources.Select(io => new InOut(io.Elements.OfType<InOutFile>().Select(file => ToInOutFile(file.Path + "foo"))));
    }

    public class ThreadIdRecordingInputProcessor : IInputProcessor {
      public int InvocationThreadId { get; private set; }

      public IObservable<InOut> Process(IObservable<InOut> sources) {
        InvocationThreadId = Thread.CurrentThread.ManagedThreadId;
        return sources;
      }
    }
  }
}