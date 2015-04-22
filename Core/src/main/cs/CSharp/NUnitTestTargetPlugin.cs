using System;
using System.IO;
using System.Threading.Tasks;
using Bud.Build;
using Bud.Dependencies;
using NUnit.ConsoleRunner;

namespace Bud.CSharp {
  public class NUnitTestTargetPlugin : CsBuild {
    public NUnitTestTargetPlugin(Key buildScope, params Setup[] extraBuildTargetSetup) : base(buildScope, extraBuildTargetSetup) {}

    protected override Settings BuildTargetSetup(Settings buildTargetSettings) {
      var mainBuildTarget = BuildTargetUtils.ProjectOf(buildTargetSettings.Scope) / BuildKeys.Main / CSharpKeys.CSharp;
      return base.BuildTargetSetup(buildTargetSettings)
                 .Add(BuildKeys.Test.Modify(TestTaskImpl),
                      CSharpKeys.AssemblyType.Modify(AssemblyType.Library),
                      DependenciesSettings.AddDependency(new CSharpInternalDependency(mainBuildTarget), config => IsMainBuildTargetDefined(config, mainBuildTarget)));
    }

    private async Task TestTaskImpl(IContext context, Func<Task> oldTest, Key buildTarget) {
      await oldTest();
      await context.Evaluate(buildTarget / CSharpKeys.Dist);
      var assemblyInDist = Path.Combine(context.GetDistDir(buildTarget), Path.GetFileName(context.GetCSharpOutputAssemblyFile(buildTarget)));
      context.Logger.Info(string.Format("Testing '{0}'...", assemblyInDist));
      var testExitCode = Runner.Main(new[] {assemblyInDist});
      if (testExitCode != 0) {
        throw new Exception(string.Format("Failed tests in '{0}'...", assemblyInDist));
      }
    }
  }
}