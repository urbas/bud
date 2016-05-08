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

namespace Bud.V1 {
  public class BuildsTest {
    [Test]
    public void DependenciesInput_must_be_empty_when_no_dependencies_given() {
      var projects = Builds.BuildProject("A", "foo");
      Assert.AreEqual(new[] {Enumerable.Empty<string>()},
               projects.Get(Builds.DependenciesOutput).ToList().Wait());
    }

    [Test]
    public void DependenciesInput_must_contain_output_from_dependencies() {
      var projects = Basic.Projects(Builds.BuildProject("A", "foo")
                                .Set(Builds.Output, Observable.Return(new[] {"a"})),
                              Builds.BuildProject("B", "boo")
                                .Add(Basic.Dependencies, "../A"));
      Assert.AreEqual(new[] {"a"},
               projects.Get("B"/Builds.DependenciesOutput).Wait());
    }


    [Test]
    public void DependenciesInput_reobserved_when_dependencies_change() {
      var testScheduler = new TestScheduler();
      var projects = Basic.Projects(Builds.BuildProject("A", "foo")
                                .Set(Basic.BuildPipelineScheduler, testScheduler)
                                .Set(Builds.Output, ChangingOutput(testScheduler)),
                              Builds.BuildProject("B", "boo")
                                .Set(Basic.BuildPipelineScheduler, testScheduler)
                                .Add(Basic.Dependencies, "../A"));
      var bInput = projects.Get("B"/Builds.DependenciesOutput).GetEnumerator();
      testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
      Assert.IsTrue(bInput.MoveNext());
      Assert.AreEqual(new[] {"foo"}, bInput.Current);
      Assert.IsTrue(bInput.MoveNext());
      Assert.AreEqual(new[] {"bar"}, bInput.Current);
      Assert.IsFalse(bInput.MoveNext());
    }

    [Test]
    public void Sources_should_be_initially_empty()
      => Assert.IsEmpty(Builds.BuildProject("A", "", "/foo").Get(Builds.Sources).Take(1).Wait());

    [Test]
    public void Input_should_initially_observe_a_single_empty_inout()
      => Assert.AreEqual(new[] {Enumerable.Empty<string>()},
                  Builds.BuildProject("A", "", "/foo").Get(Builds.Input).ToList().Wait());

    [Test]
    public void Sources_should_contain_added_files() {
      var project = Builds.SourcesSupport
                                 .AddSourceFile("A")
                                 .AddSourceFile(_ => "B");
      Assert.That(Builds.Sources[project].Take(1).Wait(),
           Is.EquivalentTo(new[] {"A", "B"}));
    }

    [Test]
    public void Sources_should_be_excluded_by_the_exclusion_filter() {
      var project = Builds.SourcesSupport
                                 .AddSourceFile("A")
                                 .AddSourceFile(_ => "B")
                                 .Add(Builds.SourceExcludeFilters, sourceFile => string.Equals("B", sourceFile));
      Assert.That(Builds.Sources[project].Take(1).Wait(),
           Is.EquivalentTo(new[] {"A"}));
    }

    [Test]
    public void Sources_should_contain_files_from_added_directories() {
      using (var tmpDir = new TemporaryDirectory()) {
        var fileA = tmpDir.CreateEmptyFile("A", "A.cs");
        var fileB = tmpDir.CreateEmptyFile("B", "B.cs");
        var twoDirsProject = Builds.BuildProject("A", "", tmpDir.Path)
          .AddSources("A")
          .AddSources("B");
        Assert.That(Builds.Sources[twoDirsProject].Take(1).Wait(),
             Is.EquivalentTo(new[] {fileA, fileB}));
      }
    }

    [Test]
    public void Sources_should_not_include_files_in_the_target_folder() {
      using (var tmpDir = new TemporaryDirectory()) {
        var project = Builds.BuildProject("A", "", tmpDir.Path)
          .AddSources(fileFilter: "*.cs");
        tmpDir.CreateEmptyFile(Basic.BuildDir[project], "A.cs");
        var files = Builds.Sources[project].Take(1).Wait();
        Assert.IsEmpty(files);
      }
    }

    [Test]
    public void Input_contains_the_added_file() {
      var buildProject = Builds.BuildProject("A", "", "/a")
        .Add(Builds.SourceIncludes, c => Builds.FilesObservatory[c].WatchFiles("foo/bar"));
      Assert.AreEqual(new[] {"foo/bar"},
               Builds.Input[buildProject].Take(1).Wait());
    }

    [Test]
    public void Source_processor_changes_source_input() {
      var fileProcessor = new Mock<IInputProcessor>(MockBehavior.Strict);
      var expectedOutputFiles = new[] {"foo"};
      fileProcessor.Setup(self => self.Process(It.IsAny<IObservable<IEnumerable<string>>>()))
                   .Returns(Observable.Return(expectedOutputFiles));
      var actualOutputFiles = Builds.BuildProject("A", "", "/a")
        .Add(Builds.SourceProcessors, fileProcessor.Object)
        .Get(Builds.ProcessedSources)
        .Wait();
      fileProcessor.VerifyAll();
      Assert.AreEqual(expectedOutputFiles, actualOutputFiles);
    }

    [Test]
    public void Source_processors_must_be_invoked_on_the_build_pipeline_thread() {
      int inputThreadId = 0;
      var fileProcessor = new ThreadIdRecordingInputProcessor();
      Builds.BuildProject("A", "", "/a")
        .Add(Builds.SourceIncludes, new FileWatcher(Enumerable.Empty<string>(), Observable.Create<string>(observer => {
          Task.Run(() => {
            inputThreadId = Thread.CurrentThread.ManagedThreadId;
            observer.OnNext("A.cs");
            observer.OnCompleted();
          });
          return new CompositeDisposable();
        })))
        .Add(Builds.SourceProcessors, fileProcessor)
        .Get(Builds.ProcessedSources).Wait();
      Assert.AreNotEqual(0, fileProcessor.InvocationThreadId);
      Assert.AreNotEqual(inputThreadId, fileProcessor.InvocationThreadId);
    }

    [Test]
    public void Default_input_contains_processed_sources() {
      var projects = Builds.BuildProject("A", "", "/a")
        .Add(Builds.SourceIncludes, new FileWatcher("b"))
        .Add(Builds.SourceProcessors, new FooAppenderInputProcessor());
      Assert.AreEqual(new[] {"bfoo"},
               projects.Get(Builds.Input).Wait());
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