using System;
using System.Threading.Tasks;
using Bud.Build;
using Bud.Dependencies;

namespace Bud.CSharp {
  public class NUnitTestTargetPlugin : CSharpBuildTargetPlugin {
    public NUnitTestTargetPlugin(Key buildScope, params Setup[] extraBuildTargetSetup) : base(buildScope, extraBuildTargetSetup) {}

    protected override Settings BuildTargetSetup(Settings buildTargetSettings) {
      var mainBuildTarget = BuildTargetUtils.ProjectOf(buildTargetSettings.Scope) / BuildKeys.Main / CSharpKeys.CSharp;
      return base.BuildTargetSetup(buildTargetSettings)
                 .Add(BuildKeys.Test.Modify(TestTaskImpl),
                      CSharpKeys.AssemblyType.Modify(AssemblyType.Library),
                      DependenciesSettings.AddDependency(new CSharpInternalDependency(mainBuildTarget), config => CSharpBuildTargetPlugin.IsMainBuildTargetDefined(config, mainBuildTarget)));
    }

    private async Task TestTaskImpl(IContext context, Func<Task> oldTest) {
      await oldTest();
    }
  }
}