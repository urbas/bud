using Bud;
using Bud.CSharp;
using Bud.Projects;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings.Add(new Project("Foo", Cs.Exe()),
                        new Macro("configKeyIntroductionMacro", ConfigKeyIntroductionMacroFunction, "this is a description of the macro."));
  }

  private static readonly ConfigKey<string> IntroducedConfig = Key.Define("introducedConfig");

  private static Settings ConfigKeyIntroductionMacroFunction(Settings settings, string[] commandLineArguments) {
    return settings.AddGlobally(IntroducedConfig.Init("Foo bar value."));
  }
}
