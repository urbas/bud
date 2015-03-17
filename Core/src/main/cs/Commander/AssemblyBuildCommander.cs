using System;
using System.IO;
using System.Reflection;
using System.Text;
using Bud.Build;
using Bud.BuildDefinition;
using Bud.Logging;

namespace Bud.Commander {
  public class AssemblyBuildCommander : MarshalByRefObject, IBuildCommander {
    public const string BuildDefinitionClassName = "Build";
    private Settings Settings;
    private IConfig Config;

    public void LoadBuildConfiguration(string buildConfigurationAssemblyFile, string baseDirectory, int buildType, TextWriter standardOutputTextWriter, TextWriter standardErrorTextWriter) {
      Console.SetOut(new NonSerializingOutputWriter(standardOutputTextWriter));
      Console.SetError(new NonSerializingOutputWriter(standardErrorTextWriter));
      var assembly = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(buildConfigurationAssemblyFile));
      var build = (IBuild) assembly.CreateInstance(BuildDefinitionClassName);
      Settings = build.Setup(GetInitialSettings(baseDirectory, buildType), baseDirectory);
      Config = new Config(Settings.ConfigDefinitions, Logger.CreateFromWriters(standardOutputTextWriter, standardErrorTextWriter));
    }

    private static Settings GetInitialSettings(string baseDirectory, int buildType) {
      switch (buildType) {
        case BuildCommanderType.ProjectLevel:
          return GlobalBuild.New(baseDirectory);
        case BuildCommanderType.BuildLevel:
          return BuildDefinitionSettings.DefaultBuildDefinitionProject(baseDirectory);
        default:
          throw new Exception("Unknown build type.");
      }
    }

    public object Evaluate(string command) {
      var context = Context.FromConfig(Config, Settings.TaskDefinitions);
      return CommandEvaluator.EvaluateSynchronously(context, command);
    }

    public void Dispose() {}
  }

  public class NonSerializingOutputWriter : TextWriter {
    private readonly TextWriter StandardOutputTextWriter;

    public NonSerializingOutputWriter(TextWriter standardOutputTextWriter) {
      StandardOutputTextWriter = standardOutputTextWriter;
    }

    public override Encoding Encoding => StandardOutputTextWriter.Encoding;

    public override void Write(object value) => base.Write(value.ToString());
  }
}