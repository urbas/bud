using Bud.Test.Assertions;
using NUnit.Framework;
using System.Linq;
using Bud.Plugins.Build;
using Bud.Plugins.Dependencies;
using Bud.Plugins.Projects;
using Bud.Plugins.CSharp;
using Bud.Plugins.BuildLoading;
using System;
using Bud.Test.Util;

namespace Bud.SystemTests {
  public class InterProjectDependencies {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var testProjectCopy = TestProjects.TemporaryCopy("InterProjectDependencies")) {
        var context = BuildLoading.Load(testProjectCopy.Path);
        Console.WriteLine(context.GetAllProjects().Select(project => context.GetDependencies(project.Value)).Aggregate((lstA, lstB) => lstA.AddRange(lstB)).Aggregate("", (str, dep) => str + "; " + dep));
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