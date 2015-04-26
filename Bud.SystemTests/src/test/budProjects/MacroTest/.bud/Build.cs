using Bud;
using Bud.CSharp;
using Bud.Projects;

public class Build : IBuild {
  private static readonly ConfigKey<string> IntroducedConfig = Key.Define("introducedConfig");
  private static readonly ConfigKey<string> IntroducedConfig2 = Key.Define("introducedConfig2");

  public Settings Setup(Settings settings, string baseDir) {
    return settings.Add(new Macro("mostGeneralDefinitionMacro", MostGeneralDefinitionMacro),
                        new Macro("settingsModifierMacro", SettingsModifierMacro, "this is a description of the macro."),
                        Macro.Valued("valueReturningMacro", ValueReturningMacro));
  }

  private static MacroResult MostGeneralDefinitionMacro(BuildContext buildContext, string[] commandLineArguments) {
    var newSettings = buildContext.Settings.AddGlobally(IntroducedConfig.Init("Something"));
    return new MacroResult("Hello, BuildContext and MacroResult World!",
                           buildContext.WithSettings(newSettings));
  }

  private static Settings SettingsModifierMacro(Settings settings, string[] commandLineArguments) {
    return settings.AddGlobally(IntroducedConfig2.Init("Foo bar value."));
  }

  private static string ValueReturningMacro(BuildContext buildContext, string[] commandLineArguments) {
    return "Hello, BuildContext Macro World!";
  }
}
