using Bud.Test.Assertions;
using NUnit.Framework;
using System.Linq;
using Bud.Plugins.Build;
using Bud.Plugins.Projects;
using Bud.Plugins.CSharp;
using Bud.Plugins.BuildLoading;
using System;
using Bud.Test.Util;

namespace Bud.SystemTests {
  public class ProjectWithDependency {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var testProjectCopy = TestProjects.TemporaryCopy("ProjectWithDependency")) {
        var context = BuildLoading.Load(testProjectCopy.Path);

        context.BuildAll().Wait();
        FileAssertions.AssertFilesExist(CompiledAssemblyFiles(context));
        Assert.AreEqual(2, CompiledAssemblyFiles(context).Count());
      }
    }

    static System.Collections.Generic.IEnumerable<string> CompiledAssemblyFiles(EvaluationContext context) {
      return context
        .GetAllProjects()
        .Select(project => context.GetCSharpOutputAssemblyFile(project.Value))
        .OrderBy(name => name);
    }
  }
}