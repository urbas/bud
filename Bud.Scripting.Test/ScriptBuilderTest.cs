using System;
using System.Collections.Immutable;
using Bud.TempDir;
using NUnit.Framework;

namespace Bud.Scripting {
  [Category("IntegrationTest")]
  public class ScriptBuilderTest {
    [Test]
    public void Generate_throws_when_unknown_reference_specified() {
      using (var dir = new TemporaryDirectory()) {
        var script = dir.CreateFile(@"//!reference INVALIDREFERENCE
public class A {public static void Main(){}}", "Build.cs");
        var exception = Assert.Throws<Exception>(() => new ScriptBuilder().Generate(dir.Path, ImmutableList.Create(script)));
        Assert.That(exception.Message,
                    Does.Contain("INVALIDREFERENCE"));
      }
    }
  }
}