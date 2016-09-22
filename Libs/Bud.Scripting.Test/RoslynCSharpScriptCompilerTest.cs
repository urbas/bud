using System;
using System.Collections.Immutable;
using Bud.References;
using NUnit.Framework;

namespace Bud.Scripting {
  public class RoslynCSharpScriptCompilerTest {
    [Test]
    public void Compile_throws_when_given_unknown_framework_assembly() {
      var exception = Assert.Throws<Exception>(() => {
        var references = new ResolvedReferences(ImmutableArray<Assembly>.Empty,
                                                ImmutableArray.Create(new FrameworkAssembly("INVALIDREFERENCE", Version.Parse("1.2.3"))));
        new RoslynCSharpScriptCompiler().Compile(ImmutableArray.Create("foo.cs"),
                                                 references,
                                                 "/foo/bar/build-script.exe");
      });
      Assert.That(exception.Message, Does.Contain("INVALIDREFERENCE"));
    }
  }
}