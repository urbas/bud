using System;
using System.Collections.Immutable;
using Bud.IO;
using NUnit.Framework;
using static Bud.Cs.Assembly;
using static Bud.Cs.CompileInput;
using static Bud.IO.InOutFile;

namespace Bud.Cs {
  public class CompileInputTest {
    [Test]
    public void FromInOut_throws_when_unknown_input_is_provided() {
      var exception = Assert.Throws<NotSupportedException>(() => FromInOut(new InOut(ImmutableList.Create(new TestInOut()))));
      Assert.That(exception.Message, Contains.Substring(typeof(TestInOut).FullName));
    }

    [Test]
    public void FromInOut_returns_empty_sources_when_given_empty_input()
      => Assert.IsEmpty(FromInOut(InOut.Empty).Sources);

    [Test]
    public void FromInOut_returns_empty_assemblies_when_given_empty_input()
      => Assert.IsEmpty(FromInOut(InOut.Empty).Assemblies);

    [Test]
    public void FromInOut_collects_sources()
      => Assert.AreEqual(new[] {"A.cs"},
                         FromInOut(new InOut(ToInOutFile("A.cs"))).Sources);

    [Test]
    public void FromInOut_collects_assemblies()
      => Assert.AreEqual(new[] {"A.dll"},
                         FromInOut(new InOut(ToAssembly("A.dll", true))).Assemblies);

    private class TestInOut : IInOut {
      public bool IsOkay { get; } = true;
    }
  }
}