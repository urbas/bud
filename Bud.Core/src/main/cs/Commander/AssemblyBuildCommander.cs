using System;
using System.IO;
using System.Reflection;
using Bud.Build;
using Bud.BuildDefinition;
using Bud.Logging;
using Bud.Util;

namespace Bud.Commander {
  public class AssemblyBuildCommander : MarshalByRefObject, IBuildCommander {
    public const string BuildDefinitionClassName = "Build";
    private Settings Settings;
    private IConfig Config;

    public void LoadBuildDefinition(string buildDefinitionAssemblyFile,
                                    string[] dependencyDlls,
                                    string baseDirectory,
                                    int buildLevel,
                                    TextWriter standardOutputTextWriter,
                                    TextWriter standardErrorTextWriter) {
      Console.SetOut(standardOutputTextWriter);
      Console.SetError(standardErrorTextWriter);
      AppDomain.CurrentDomain.AssemblyResolve += AssemblyUtils.PathListAssemblyResolver(dependencyDlls);
      var assembly = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(buildDefinitionAssemblyFile));
      var build = (IBuild) assembly.CreateInstance(BuildDefinitionClassName);
      Settings = build.Setup(GetInitialSettings(baseDirectory, buildLevel), baseDirectory);
      Config = new Config(Settings.ConfigDefinitions, Logger.CreateFromWriters(standardOutputTextWriter, standardErrorTextWriter));
    }

    private static Settings GetInitialSettings(string baseDirectory, int buildLevel) {
      switch (buildLevel) {
        case BuildCommanderType.ProjectLevel:
          return GlobalBuild.New(baseDirectory);
        case BuildCommanderType.BuildLevel:
          return BuildDefinitionSettings.DefaultBuildDefinitionProject(baseDirectory);
        default:
          throw new Exception("Unknown build type.");
      }
    }

    public string EvaluateToJson(string command) {
      var context = Context.FromConfig(Config, Settings.TaskDefinitions);
      return CommandEvaluator.EvaluateToJsonSynchronously(context, command);
    }

    public void Dispose() {}
  }
}