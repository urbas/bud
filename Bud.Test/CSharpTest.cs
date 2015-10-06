using System;
using System.IO;
using Bud.IO;
using Bud.Tasking.ApiV1;
using NUnit.Framework;
using static Bud.Build;
using static Bud.CSharp;

namespace Bud {
  public class CSharpTest
  {
    private TemporaryDirectory tempDir;
    private Tasks project;

    [SetUp]
    public void SetUp() {
      tempDir = new TemporaryDirectory();
      project = Project(tempDir.Path, "Foo")
        .ExtendWith(Sources(fileFilter: "*.cs"))
        .ExtendWith(RoslynCompiler());
    }

    [TearDown]
    public void TearDown() => tempDir.Dispose();

    [Test]
    public async void Compiles_a_single_source_file() {
      tempDir.CreateFile(@"public class A {}", "A.cs");
      var compilationResult = await Compile[project.ToResultCachingTasks()];
      Assert.IsTrue(File.Exists(compilationResult.AssemblyPath));
      Assert.IsTrue(compilationResult.EmitResult.Success);
      Assert.IsEmpty(compilationResult.EmitResult.Diagnostics);
    }
  }
}