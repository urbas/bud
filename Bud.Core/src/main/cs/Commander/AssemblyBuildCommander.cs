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
    private IBuildContext BuildContext;
    private ICommandEvaluator CommandEvaluator = new CommandEvaluator();

    public void LoadBuildDefinition(string buildDefinitionAssemblyFile,
                                    string[] dependencyDlls,
                                    string baseDirectory,
                                    int buildLevel,
                                    TextWriter outputTextWriter,
                                    TextWriter errorTextWriter) {
      Console.SetOut(outputTextWriter);
      Console.SetError(errorTextWriter);
      AppDomain.CurrentDomain.AssemblyResolve += AssemblyUtils.PathListAssemblyResolver(dependencyDlls);
      BuildContext = CreateBuildContext(buildDefinitionAssemblyFile, baseDirectory, buildLevel, outputTextWriter, errorTextWriter);
    }

    public string EvaluateToJson(string command)
      => CommandEvaluator.EvaluateToJsonSync(command, ref BuildContext);

    public string EvaluateMacroToJson(string macroName, params string[] commandLineParameters)
      => CommandEvaluator.EvaluateMacroToJsonSync(macroName, commandLineParameters, ref BuildContext);

    public void Dispose() {}

    private static BuildContext CreateBuildContext(string buildDefinitionAssemblyFile, string baseDirectory, int buildLevel, TextWriter outputTextWriter, TextWriter errorTextWriter) {
      var assembly = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(buildDefinitionAssemblyFile));
      var build = (IBuild) assembly.CreateInstance(BuildDefinitionClassName);
      var settings = build.Setup(GetInitialSettings(baseDirectory, buildLevel), baseDirectory);
      var logger = Logger.CreateFromWriters(outputTextWriter, errorTextWriter);
      return new BuildContext(settings, logger);
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
  }
}