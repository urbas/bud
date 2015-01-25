using System;
using System.IO;
using System.Reflection;
using Bud.Build;

namespace Bud.Commander {
  public class AssemblyBuildCommander : MarshalByRefObject, IBuildCommander {
    private Settings settings;
    private IConfig config;

    public void LoadBuildConfiguration(string buildConfigurationAssemblyFile, string baseDirectory, TextWriter standardOutputTextWriter, TextWriter standardErrorTextWriter) {
      Console.SetOut(standardOutputTextWriter);
      Console.SetError(standardErrorTextWriter);
      var assembly = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(buildConfigurationAssemblyFile));
      var build = (IBuild) assembly.CreateInstance("Build");
      settings = build.Setup(GlobalBuild.New(baseDirectory), baseDirectory);
      config = new Config(settings.ConfigDefinitions);
    }

    public object Evaluate(string command) {
      var context = Context.FromConfig(config, settings.TaskDefinitions);
      return CommandEvaluator.EvaluateSynchronously(context, command);
    }

    public void Dispose() {}
  }
}