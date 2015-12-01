using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.IO;
using NUnit.Framework;
using static Bud.Cs.Assembly;
using static Bud.Cs.CompileInput;
using static Bud.IO.InOutFile;

namespace Bud.Cs {
  public class CompileInputTest {
    private List<Timestamped<string>> sources;
    private List<Timestamped<string>> assemblies;

    [Test]
    public void FromInOut_throws_when_unknown_input_is_provided() {
      var exception = Assert.Throws<NotSupportedException>(() => ExtractInput(new InOut(ImmutableList.Create(new TestInOut())), out sources, out assemblies));
      Assert.That(exception.Message, Contains.Substring(typeof(TestInOut).FullName));
    }

    [Test]
    public void FromInOut_returns_empty_sources_when_given_empty_input() {
      ExtractInput(InOut.Empty, out sources, out assemblies);
      Assert.IsEmpty(sources);
    }

    [Test]
    public void FromInOut_returns_empty_assemblies_when_given_empty_input() {
      ExtractInput(InOut.Empty, out sources, out assemblies);
      Assert.IsEmpty(assemblies);
    }

    [Test]
    public void FromInOut_collects_sources() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A.cs");
        ExtractInput(new InOut(ToInOutFile(fileA, true)), out sources, out assemblies);
        Assert.AreEqual(new[] {Files.ToTimestampedFile(fileA)},
                        sources);
      }
    }

    [Test]
    public void FromInOut_collects_assemblies() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A.dll");
        ExtractInput(new InOut(ToAssembly(fileA, true)), out sources, out assemblies);
        Assert.AreEqual(new[] {Files.ToTimestampedFile(fileA)},
                        assemblies);
      }
    }

    private class TestInOut : IInOut {
      public bool IsOkay { get; } = true;
    }
  }
}