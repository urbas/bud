using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Bud.Plugins.Build;
using Bud.Plugins.BuildLoading;

namespace Bud.Commander {
  public class AssemblyBuildCommander : MarshalByRefObject, IBuildCommander {
    private Settings settings;

    public void LoadBuildConfiguration(string buildConfigurationAssemblyFile, string baseDirectory) {
      var assembly = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(buildConfigurationAssemblyFile));
      var build = (IBuild)assembly.CreateInstance("Build");
      settings = build.GetSettings(baseDirectory);
    }

    public string Evaluate(string command) {
      return CommandEvaluator.Evaluate(settings, command);
    }

    public void Dispose() {
    }
  }
}