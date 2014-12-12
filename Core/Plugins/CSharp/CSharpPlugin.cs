using Bud.Plugins.CSharp.Compiler;
using Bud.Plugins.Projects;
using System.IO;
using Bud.Util;
using Bud.Plugins.Build;
using Bud.Plugins.Dependencies;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Bud.Plugins.CSharp {

  public class CSharpPlugin : IPlugin {
    public static readonly CSharpPlugin Instance = new CSharpPlugin();

    private CSharpPlugin() {
    }

    public Settings ApplyTo(Settings settings, Scope scope) {
      return settings
        .Add(BuildPlugin.Instance)
        .Add(DependenciesPlugin.Instance)
        .InitOrKeep(CSharpKeys.Build.In(scope), ctxt => MonoCompiler.CompileProject(ctxt, scope))
        .InitOrKeep(CSharpKeys.Build, TaskUtils.NoOpTask)
        .InitOrKeep(CSharpKeys.SourceFiles.In(scope), context => FindSources(context, scope))
        .InitOrKeep(CSharpKeys.AssemblyType.In(scope), AssemblyType.Exe)
        .InitOrKeep(CSharpKeys.CollectReferencedAssemblies.In(scope), context => CollectAssembliesFromDependencies(context, scope))
        .InitOrKeep(CSharpKeys.OutputAssemblyDir.In(scope), context => Path.Combine(context.GetOutputDir(scope), ".net-4.5", "main", "debug", "bin"))
        .InitOrKeep(CSharpKeys.OutputAssemblyName.In(scope), context => scope.Id)
        .InitOrKeep(CSharpKeys.OutputAssemblyFile.In(scope), context => Path.Combine(context.GetCSharpOutputAssemblyDir(scope), string.Format("{0}.{1}", context.GetCSharpOutputAssemblyName(scope), context.GetAssemblyFileExtension(scope))))
        .AddDependencies(CSharpKeys.Build, CSharpKeys.Build.In(scope))
        .AddDependencies(BuildKeys.Build, CSharpKeys.Build);
    }

    public static async Task<ImmutableList<string>> CollectAssembliesFromDependencies(EvaluationContext context, Scope currentProject) {
      var collectedAssemblies = ImmutableList<string>.Empty;
      var dependencies = context.GetScopeDependencies(currentProject);
      await Task.WhenAll(dependencies.Select(dependency => context.CSharpBuild(dependency.Scope)));
      foreach (var dependency in dependencies) {
        collectedAssemblies = collectedAssemblies.Add(context.GetCSharpOutputAssemblyFile(dependency.Scope));
      }
      return collectedAssemblies;
    }

    public IEnumerable<string> FindSources(EvaluationContext context, Scope scope) {
      var sourceDirectory = context.GetCSharpSourceDir(scope);
      if (Directory.Exists(sourceDirectory)) {
        return Directory.EnumerateFiles(sourceDirectory);
      } else {
        return ImmutableList<string>.Empty;
      }
    }
  }
}

