using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using static Bud.Cs.Assembly;
using static Bud.Cs.CompileInput;
using static Bud.IO.InOutFile;

namespace Bud.Cs {
  public class CompileInputTest {
    private List<Timestamped<string>> sources;
    private List<Timestamped<string>> assemblies;
    private List<CompileOutput> dependencies;

    [Test]
    public void FromInOut_throws_when_unknown_input_is_provided() {
      var exception = Assert.Throws<NotSupportedException>(() => ExtractInput(new InOut(ImmutableList.Create(new TestInOut())), out sources, out assemblies, out dependencies));
      Assert.That(exception.Message, Contains.Substring(typeof (TestInOut).FullName));
    }

    [Test]
    public void FromInOut_returns_empty_sources_when_given_empty_input() {
      ExtractInput(InOut.Empty, out sources, out assemblies, out dependencies);
      Assert.IsEmpty(sources);
    }

    [Test]
    public void FromInOut_returns_empty_assemblies_when_given_empty_input() {
      ExtractInput(InOut.Empty, out sources, out assemblies, out dependencies);
      Assert.IsEmpty(assemblies);
    }

    [Test]
    public void FromInOut_collects_sources() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A.cs");
        ExtractInput(new InOut(ToInOutFile(fileA)), out sources, out assemblies, out dependencies);
        Assert.AreEqual(new[] {Files.ToTimestampedFile(fileA)},
                        sources);
      }
    }

    [Test]
    public void FromInOut_collects_assemblies() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A.dll");
        ExtractInput(new InOut(ToAssembly(fileA)), out sources, out assemblies, out dependencies);
        Assert.AreEqual(new[] {Files.ToTimestampedFile(fileA)},
                        assemblies);
      }
    }

    [Test]
    public void FromInOut_collects_compile_outputs_from_dependencies() {
      var dependency = FooCompileOutput();
      ExtractInput(new InOut(dependency), out sources, out assemblies, out dependencies);
      Assert.AreEqual(new[] {dependency},
                      dependencies);
    }

    [Test]
    public void FromInOut_collects_compile_outputs_into_assemblies() {
      var dependency = FooCompileOutput();
      ExtractInput(new InOut(dependency), out sources, out assemblies, out dependencies);
      Assert.AreEqual(new[] {Timestamped.Create(dependency.AssemblyPath, 0L) },
                      assemblies);
    }

    private static CompileOutput FooCompileOutput()
      => new CompileOutput(Enumerable.Empty<Diagnostic>(), TimeSpan.Zero, "Foo.dll", true, 0L, null);

    private class TestInOut {}
  }
}