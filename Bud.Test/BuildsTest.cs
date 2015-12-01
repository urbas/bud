using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bud.Configuration.ApiV1;
using Bud.IO;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using static System.Linq.Enumerable;
using static System.TimeSpan;
using static Bud.Builds;
using static Bud.IO.InOutFile;

namespace Bud {
  public class BuildsTest {
    private readonly Conf project = Project("bar", "Foo");

    [Test]
    public void Set_the_projectDir() => Assert.AreEqual("bar", ProjectDir[project]);

    [Test]
    public void Set_the_projectId() => Assert.AreEqual("Foo", ProjectId[project]);

    [Test]
    public void Sources_should_be_initially_empty()
      => Assert.IsEmpty(Sources[project].Lister);

    [Test]
    public void Dependencies_should_be_initially_empty()
      => Assert.IsEmpty(Dependencies[project]);

    [Test]
    public void Input_should_initially_observe_a_single_empty_inout()
      => Assert.AreEqual(new [] {InOut.Empty}, Input[project].ToList().Wait());

    [Test]
    public void Multiple_source_directories() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A", "A.cs");
        var fileB = tempDir.CreateEmptyFile("B", "B.cs");
        var twoDirsProject = Project(tempDir.Path, "foo").Add(SourceDir("A"), SourceDir("B"));
        Assert.That(Sources[twoDirsProject].Lister,
                    Is.EquivalentTo(new[] {fileA, fileB}));
      }
    }

    [Test]
    public void Source_processor_changes_source_input() {
      var fileProcessor = new Mock<IFilesProcessor>(MockBehavior.Strict);
      var expectedOutputFiles = new InOut(ToInOutFile("foo"));
      fileProcessor.Setup(self => self.Process(It.IsAny<IObservable<InOut>>()))
                   .Returns(Observable.Return(expectedOutputFiles));
      var actualOutputFiles = Project("FooDir", "Foo")
        .AddSourceProcessor(conf => fileProcessor.Object)
        .Get(ProcessedSources)
        .Wait();
      fileProcessor.VerifyAll();
      Assert.AreEqual(expectedOutputFiles, actualOutputFiles);
    }

    [Test]
    public void Source_processors_must_be_invoked_on_the_build_pipeline_thread() {
      int inputThreadId = 0;
      var fileProcessor = new ThreadIdRecordingFileProcessor();
      Project("fooDir", "A")
        .SetValue(Sources, new Files(Empty<string>(), Observable.Create<string>(observer => {
          Task.Run(() => {
            inputThreadId = Thread.CurrentThread.ManagedThreadId;
            observer.OnNext("foo");
            observer.OnCompleted();
          });
          return new CompositeDisposable();
        })))
        .AddSourceProcessor(_ => fileProcessor)
        .Get(ProcessedSources).Wait();
      Assert.AreNotEqual(0, fileProcessor.InvocationThreadId);
      Assert.AreNotEqual(inputThreadId, fileProcessor.InvocationThreadId);
    }

    [Test]
    public void Default_build_forwards_the_output_from_dependencies() {
      var projects = Conf.New(Project("aDir", "A")
                                .SetValue(Sources, new Files("a")),
                              Project("bDir", "B")
                                .SetValue(Sources, new Files("b"))
                                .Add(Dependencies, "../A"));
      Assert.AreEqual(new InOut(ToInOutFile("a"), ToInOutFile("b")),
                      projects.Get("B" / Output).Wait());
    }

    [Test]
    public void Input_reobserved_when_dependencies_change() {
      var testScheduler = new TestScheduler();
      var projects = Conf.New(Project("aDir", "A")
                                .SetValue(BuildPipelineScheduler, testScheduler)
                                .SetValue(Sources, new Files(Empty<string>(), Observable.Return("foo").Delay(FromSeconds(1), testScheduler)
                                                                                        .Concat(Observable.Return("bar").Delay(FromSeconds(1), testScheduler)))),
                              Project("bDir", "B")
                                .SetValue(BuildPipelineScheduler, testScheduler)
                                .Add(Dependencies, "../A"));
      var output = projects.Get("B" / Output).GetEnumerator();
      testScheduler.AdvanceBy(FromSeconds(5).Ticks);
      Assert.IsTrue(output.MoveNext());
      Assert.IsTrue(output.MoveNext());
      Assert.IsTrue(output.MoveNext());
    }

    [Test]
    public void Default_build_processes_own_sources_before_output() {
      var projects = Conf.New(Project("aDir", "A")
                                .SetValue(Sources, new Files("a"))
                                .AddSourceProcessor(conf => new FooAppenderFileProcessor()),
                              Project("bDir", "B")
                                .SetValue(Sources, new Files("b"))
                                .AddSourceProcessor(conf => new FooAppenderFileProcessor())
                                .Add(Dependencies, "../A"));
      Assert.AreEqual(new InOut(ToInOutFile("afoo"), ToInOutFile("bfoo")),
                      projects.Get("B" / Output).Wait());
    }

    private class FooAppenderFileProcessor : IFilesProcessor {
      public IObservable<InOut> Process(IObservable<InOut> sources)
        => sources.Select(io => new InOut(io.Elements.OfType<InOutFile>().Select(file => ToInOutFile(file.Path + "foo"))));
    }

    public class ThreadIdRecordingFileProcessor : IFilesProcessor {
      public int InvocationThreadId { get; private set; }

      public IObservable<InOut> Process(IObservable<InOut> sources) {
        InvocationThreadId = Thread.CurrentThread.ManagedThreadId;
        return sources;
      }
    }
  }
}