using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bud.Build;
using Bud.Dependencies;
using NUnit.ConsoleRunner;

namespace Bud.CSharp {
  public class NUnitTestTargetPlugin : CsBuild {
    public static readonly ConfigKey<List<string>> NUnitArgumentsKey = Key.Define("nunitArgs", "Arguments that will be passed to NUnit.");

    public NUnitTestTargetPlugin(Key buildScope, params Setup[] extraBuildTargetSetup) : base(buildScope, extraBuildTargetSetup) {}

    protected override Settings BuildTargetSetup(Settings buildTargetSettings) {
      var mainBuildTarget = BuildTargetUtils.ProjectOf(buildTargetSettings.Scope) / BuildKeys.Main / CSharpKeys.CSharp;
      return base.BuildTargetSetup(buildTargetSettings)
                 .Add(BuildKeys.Test.Modify(TestTaskImpl),
                      CSharpKeys.AssemblyType.Modify(AssemblyType.Library),
                      NUnitArgumentsKey.Init(DefaultNUnitArguments),
                      DependenciesSettings.AddDependency(new CSharpInternalDependency(mainBuildTarget), config => IsMainBuildTargetDefined(config, mainBuildTarget)));
    }

    private static List<string> DefaultNUnitArguments(IConfig config, Key buildTarget) {
      var assemblyFileName = Path.GetFileName(config.GetCSharpOutputAssemblyFile(buildTarget));
      var assemblyInDist = Path.Combine(config.GetDistDir(buildTarget), assemblyFileName);
      return new List<string> {assemblyInDist};
    }

    private static async Task TestTaskImpl(IContext context, Func<Task> oldTest, Key buildTarget) {
      await oldTest();
      // NOTE: we perform 'dist' to ensure that all dependencies are in the same directory as the assembly under test.
      await context.Evaluate(buildTarget / CSharpKeys.Dist);
      context.Logger.Info(string.Format("Testing '{0}'...", buildTarget));
      var nunitArguments = context.Evaluate(buildTarget / NUnitArgumentsKey);
      var testExitCode = Runner.Main(nunitArguments.ToArray());
      if (testExitCode != 0) {
        throw new Exception(string.Format("Failed tests in '{0}'...", buildTarget));
      }
    }
  }
}