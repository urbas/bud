using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Bud.IO;
using Moq;
using NUnit.Framework;
using static Bud.Build;

namespace Bud {
  public class BuildTest {
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
      var expectedOutputFiles = ImmutableArray.Create(Timestamped.Create("foo", 42L));
      fileProcessor.Setup(self => self.Process(It.IsAny<IObservable<ImmutableArray<Timestamped<string>>>>()))
                   .Returns(Observable.Return(expectedOutputFiles));
      var actualOutputFiles = Project("FooDir", "Foo")
        .AddSourceProcessor(conf => fileProcessor.Object)
        .Get(SourceInput)
        .Wait();
      fileProcessor.VerifyAll();
      Assert.AreEqual(expectedOutputFiles, actualOutputFiles);
    }
  }
}