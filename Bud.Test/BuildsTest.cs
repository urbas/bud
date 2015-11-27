using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bud.Configuration.ApiV1;
using Bud.IO;
using Moq;
using NUnit.Framework;
using static Bud.Builds;

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
    public void Multiple_source_directories() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A", "A.cs");
        var fileB = tempDir.CreateEmptyFile("B", "B.cs");
        var twoDirsProject = Project(tempDir.Path, "foo").Add(SourceDir("A"), SourceDir("B"));
        Assert.That(Sources[twoDirsProject].Lister,
                    Is.EquivalentTo(new[] {Files.ToTimestampedFile(fileA), Files.ToTimestampedFile(fileB)}));
      }
    }

    [Test]
    public void Source_processor_changes_source_input() {
      var fileProcessor = new Mock<IFilesProcessor>(MockBehavior.Strict);
      var expectedOutputFiles = new[] {"foo"};
      fileProcessor.Setup(self => self.Process(It.IsAny<IObservable<IEnumerable<string>>>()))
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
        .SetValue(Sources, new Files(Enumerable.Empty<string>(), Observable.Create<string>(observer => {
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
    public void Default_build_forwards_the_sources_from_dependencies() {
      var projects = Conf.New(Project("aDir", "A")
                                .SetValue(Sources, new Files(new[] {"a"})),
                              Project("bDir", "B")
                                .SetValue(Sources, new Files(new[] {"b"}))
                                .Add(Dependencies, "../A"));
      Assert.AreEqual(new[] {Timestamped.Create("a", 0L), Timestamped.Create("b", 0L)},
                      projects.Get("B" / Output).Wait());
    }

    [Test]
    public void Default_build_processes_own_sources_before_output() {
      var projects = Conf.New(Project("aDir", "A")
                                .SetValue(Sources, new Files(new[] {"a"}))
                                .AddSourceProcessor(conf => new FooAppenderFileProcessor()),
                              Project("bDir", "B")
                                .SetValue(Sources, new Files(new[] {"b"}))
                                .AddSourceProcessor(conf => new FooAppenderFileProcessor())
                                .Add(Dependencies, "../A"));
      Assert.AreEqual(new[] {Timestamped.Create("afoo", 0L), Timestamped.Create("bfoo", 0L)},
                      projects.Get("B" / Output).Wait());
    }

    private class FooAppenderFileProcessor : IFilesProcessor {
      public IObservable<IEnumerable<string>> Process(IObservable<IEnumerable<string>> sources) {
        return sources.Select(files => files.Select(file => file + "foo"));
      }
    }

    public class ThreadIdRecordingFileProcessor : IFilesProcessor {
      public int InvocationThreadId { get; private set; }

      public IObservable<IEnumerable<string>> Process(IObservable<IEnumerable<string>> sources) {
        InvocationThreadId = Thread.CurrentThread.ManagedThreadId;
        return sources;
      }
    }
  }
}