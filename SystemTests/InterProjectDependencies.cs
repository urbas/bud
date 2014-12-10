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
using Bud.Commander;

namespace Bud.SystemTests {
  public class InterProjectDependencies {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var buildCommander = TestProjects.LoadBuildCommander("InterProjectDependencies")) {
        buildCommander.Evaluate(BuildKeys.Build);
//        Console.WriteLine(context.GetAllProjects().Select(project => context.GetDependencies(project.Value)).Aggregate((lstA, lstB) => lstA.AddRange(lstB)).Aggregate("", (str, dep) => str + "; " + dep));
//        FileAssertions.AssertFilesExist(CompiledAssemblyFiles(builder));
//        Assert.AreEqual(2, CompiledAssemblyFiles(builder).Count());
      }
    }

//    static System.Collections.Generic.IEnumerable<string> CompiledAssemblyFiles(TemporaryDirBuilder builder) {
//      return context
//        .GetAllProjects()
//        .Select(project => context.GetCSharpOutputAssemblyFile(project.Value))
//        .OrderBy(name => name);
//    }
  }
}