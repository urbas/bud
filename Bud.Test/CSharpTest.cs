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
    public async void Compiles_a_single_source_file() {
      var cSharpCompiler = new Mock<ICSharpCompiler>();
      var compilationResult = new Mock<ICompilationResult>();
      var fileSystemObserverFactory = new MockFileSystemObserverFactory();

      var project = Project("fooDir", "Foo", fileSystemObserverFactory)
        .ExtendWith(Sources("A.cs"))
        .ExtendWith(CSharpCompilation(cSharpCompiler.Object, observable => observable));

      cSharpCompiler.Setup(self => self.Compile(It.IsAny<string>(), It.IsAny<string>(), It.Is((IFiles files) => files.Contains("A.cs")), It.IsAny<CSharpCompilationOptions>(), It.IsAny<IEnumerable<MetadataReference>>()))
                    .Returns(compilationResult.Object);

      Assert.AreEqual(compilationResult.Object, await project.Get(Compile).Take(1).ToTask());
    }
  }
}