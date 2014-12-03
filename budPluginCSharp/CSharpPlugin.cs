using Bud.Plugins.CSharp.Compiler;
using Bud.Plugins.Projects;
using System.IO;
using Bud.Util;
using Bud.Plugins.Build;

namespace Bud.Plugins.CSharp {

  public class CSharpPlugin : BudPlugin {
    public static readonly CSharpPlugin Instance = new CSharpPlugin();

    private CSharpPlugin() {
    }

    public Settings ApplyTo(Settings settings, Scope scope) {
      return settings
        .Add(BuildPlugin.Instance)
        .InitOrKeep(CSharpKeys.Build, TaskUtils.NoOpTask)
        .AddDependencies(BuildKeys.Build, CSharpKeys.Build)
        .InitOrKeep(CSharpKeys.Build.In(scope), ctxt => MonoCompiler.CompileProject(ctxt, scope))
        .AddDependencies(CSharpKeys.Build, CSharpKeys.Build.In(scope));
    }
  }
}

