using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Bud.Compilation;
using Bud.IO;
using Bud.Tasking.ApiV1;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using NUnit.Framework;
using static Bud.Build;
using static Bud.CSharp;

namespace Bud {
  public class CSharpTest {
    [Test]
    public void Invokes_the_compiler() {
      var cSharpCompiler = new Mock<ICSharpCompiler>();
      var compilationResult = Observable.Return(new Mock<ICompilationResult>().Object);

      var project = Project("fooDir", "Foo").ExtendWith(CSharpCompilation(cSharpCompiler.Object));

      cSharpCompiler.Setup(self => self.Compile(It.IsAny<IObservable<IFiles>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CSharpCompilationOptions>(), It.IsAny<IEnumerable<MetadataReference>>()))
                    .Returns(compilationResult);

      Assert.AreSame(compilationResult, project.Get(Compile));
    }
  }
}