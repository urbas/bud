using System;
using System.Collections.Immutable;
using System.IO;
using Bud.IO;
using Moq;
using NUnit.Framework;

namespace Bud.Build {
  public class TimestampBuilderTest {
    private TemporaryDirectory dir;
    private Mock<IBuilder> worker;

    [SetUp]
    public void SetUp() {
      worker = new Mock<IBuilder>(MockBehavior.Strict);
      dir = new TemporaryDirectory();
    }

    [TearDown]
    public void TearDown() => dir.Dispose();

    [Test]
    public void Build_creates_the_output_file() {
      var output = dir.CreatePath("a.out");
      worker.Setup(self => self.Build(output, ImmutableList<string>.Empty));
      TimestampBuilder.Build(worker.Object, output);
      worker.VerifyAll();
    }

    [Test]
    public void Build_does_not_invoke_the_worker_when_output_already_exists() {
      var output = dir.CreateEmptyFile("a.out");
      TimestampBuilder.Build(worker.Object, output);
    }

    [Test]
    public void Build_invokes_the_worker_when_the_output_is_stale() {
      var output = dir.CreateEmptyFile("a.out");
      var fileA = dir.CreateFile("foo", "a");
      File.SetLastWriteTime(output, File.GetLastWriteTime(fileA) - TimeSpan.FromSeconds(1));
      var input = ImmutableList.Create(fileA);
      worker.Setup(self => self.Build(output, input));

      TimestampBuilder.Build(worker.Object, output, input);

      worker.VerifyAll();
    }

    [Test]
    public void Build_does_not_invoke_the_worker_when_the_output_is_up_to_date() {
      var fileA = dir.CreateFile("foo", "a");
      var output = dir.CreateEmptyFile("a.out");
      File.SetLastWriteTime(fileA, File.GetLastWriteTime(output) - TimeSpan.FromSeconds(1));
      var inputFiles = ImmutableList.Create(fileA);
      TimestampBuilder.Build(worker.Object, output, inputFiles);
    }
  }
}