using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Bud.Plugins.Build;
using Bud.Plugins.BuildLoading;
using System.IO;

namespace Bud.Commander {
  public class AssemblyBuildCommander : MarshalByRefObject, IBuildCommander {
    private Settings settings;
    private IConfig config;

    public void LoadBuildConfiguration(string buildConfigurationAssemblyFile, string baseDirectory, TextWriter standardOutputTextWriter, TextWriter standardErrorTextWriter) {
      Console.SetOut(standardOutputTextWriter);
      Console.SetError(standardErrorTextWriter);
      var assembly = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(buildConfigurationAssemblyFile));
      var build = (IBuild)assembly.CreateInstance("Build");
      settings = build.SetUp(GlobalBuild.New(baseDirectory), baseDirectory);
      config = new Config(settings.ConfigDefinitions);
      Console.WriteLine(Context.FromConfig(config, settings.TaskDefinitions).ToString());
    }

    public object Evaluate(string command) {
      var context = Context.FromConfig(config, settings.TaskDefinitions);
      return CommandEvaluator.EvaluateSynchronously(context, command);
    }

    public void Dispose() {
    }
  }
}
