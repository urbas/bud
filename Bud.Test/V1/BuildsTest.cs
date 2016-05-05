using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bud.IO;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using static Bud.V1.Builds;

namespace Bud.V1 {
  public class BuildsTest {
    [Test]
    public void DependenciesInput_must_be_empty_when_no_dependencies_given() {
      var projects = BuildProject("A", "foo");
      Assert.AreEqual(new[] {Enumerable.Empty<string>()},
               projects.Get(DependenciesOutput).ToList().Wait());
    }

    [Test]
    public void DependenciesInput_must_contain_output_from_dependencies() {
      var projects = Basic.Projects(BuildProject("A", "foo")
                                .Set(Output, Observable.Return(new[] {"a"})),
                              BuildProject("B", "boo")
                                .Add(Basic.Dependencies, "../A"));
      Assert.AreEqual(new[] {"a"},
               projects.Get("B"/DependenciesOutput).Wait());
    }


    [Test]
    public void DependenciesInput_reobserved_when_dependencies_change() {
      var testScheduler = new TestScheduler();
      var projects = Basic.Projects(BuildProject("A", "foo")
                                .Set(Basic.BuildPipelineScheduler, testScheduler)
                                .Set(Output, ChangingOutput(testScheduler)),
                              BuildProject("B", "boo")
                                .Set(Basic.BuildPipelineScheduler, testScheduler)
                                .Add(Basic.Dependencies, "../A"));
      var bInput = projects.Get("B"/DependenciesOutput).GetEnumerator();
      testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
      Assert.IsTrue(bInput.MoveNext());
      Assert.AreEqual(new[] {"foo"}, bInput.Current);
      Assert.IsTrue(bInput.MoveNext());
      Assert.AreEqual(new[] {"bar"}, bInput.Current);
      Assert.IsFalse(bInput.MoveNext());
    }

    [Test]
    public void Sources_should_be_initially_empty()
      => Assert.IsEmpty(BuildProject("A", "", "/foo").Get(Sources).Take(1).Wait());

    [Test]
    public void Input_should_initially_observe_a_single_empty_inout()
      => Assert.AreEqual(new[] {Enumerable.Empty<string>()},
                  BuildProject("A", "", "/foo").Get(Input).ToList().Wait());

    [Test]
    public void Sources_should_contain_added_files() {
      var project = SourcesSupport
                                 .AddSourceFile("A")
                                 .AddSourceFile(_ => "B");
      Assert.That(Sources[project].Take(1).Wait(),
           Is.EquivalentTo(new[] {"A", "B"}));
    }

    [Test]
    public void Sources_should_be_excluded_by_the_exclusion_filter() {
      var project = SourcesSupport
                                 .AddSourceFile("A")
                                 .AddSourceFile(_ => "B")
                                 .Add(SourceExcludeFilters, sourceFile => string.Equals("B", sourceFile));
      Assert.That(Sources[project].Take(1).Wait(),
           Is.EquivalentTo(new[] {"A"}));
    }

    [Test]
    public void Sources_should_contain_files_from_added_directories() {
      using (var tmpDir = new TemporaryDirectory()) {
        var fileA = tmpDir.CreateEmptyFile("A", "A.cs");
        var fileB = tmpDir.CreateEmptyFile("B", "B.cs");
        var twoDirsProject = BuildProject("A", "", tmpDir.Path)
          .AddSources("A")
          .AddSources("B");
        Assert.That(Sources[twoDirsProject].Take(1).Wait(),
             Is.EquivalentTo(new[] {fileA, fileB}));
      }
    }

    [Test]
    public void Sources_should_not_include_files_in_the_target_folder() {
      using (var tmpDir = new TemporaryDirectory()) {
        var project = BuildProject("A", "", tmpDir.Path)
          .AddSources(fileFilter: "*.cs");
        tmpDir.CreateEmptyFile(Basic.BuildDir[project], "A.cs");
        var files = Sources[project].Take(1).Wait();
        Assert.IsEmpty(files);
      }
    }

    [Test]
    public void Input_contains_the_added_file() {
      var buildProject = BuildProject("A", "", "/a")
        .Add(SourceIncludes, c => FilesObservatory[c].WatchFiles("foo/bar"));
      Assert.AreEqual(new[] {"foo/bar"},
               Input[buildProject].Take(1).Wait());
    }

    [Test]
    public void Source_processor_changes_source_input() {
      var fileProcessor = new Mock<IInputProcessor>(MockBehavior.Strict);
      var expectedOutputFiles = new[] {"foo"};
      fileProcessor.Setup(self => self.Process(It.IsAny<IObservable<IEnumerable<string>>>()))
                   .Returns(Observable.Return(expectedOutputFiles));
      var actualOutputFiles = BuildProject("A", "", "/a")
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
      BuildProject("A", "", "/a")
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
      Assert.AreNotEqual(0, fileProcessor.InvocationThreadId);
      Assert.AreNotEqual(inputThreadId, fileProcessor.InvocationThreadId);
    }

    [Test]
    public void Default_input_contains_processed_sources() {
      var projects = BuildProject("A", "", "/a")
        .Add(SourceIncludes, new FileWatcher("b"))
        .Add(SourceProcessors, new FooAppenderInputProcessor());
      Assert.AreEqual(new[] {"bfoo"},
               projects.Get(Input).Wait());
    }

    [Test]
    public void Clean_deletes_non_empty_target_folders() {
      using (var tmpDir = new TemporaryDirectory()) {
        tmpDir.CreateEmptyFile("build", "A", "foo", "foo.txt");
        BuildProject("A", "", tmpDir.Path).Get(Basic.Clean);
        Assert.IsFalse(Directory.Exists(Path.Combine(tmpDir.Path, "build", "A")));
      }
    }

    [Test]
    public void Clean_does_nothing_when_the_target_folder_does_not_exist() {
      using (var tmpDir = new TemporaryDirectory()) {
        BuildProject("A", "", tmpDir.Path).Get(Basic.Clean);
      }
    }

    private static IObservable<string[]> ChangingOutput(IScheduler scheduler)
      => Observable.Return(new[] {"foo"}).Delay(TimeSpan.FromSeconds(1), scheduler)
                              .Concat(Observable.Return(new[] {"bar"})
                                        .Delay(TimeSpan.FromSeconds(1), scheduler));

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