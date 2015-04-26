using Bud;
using Bud.CSharp;
using Bud.Projects;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings.Add(new Project("Foo", Cs.Exe()),
                        new Macro("configKeyIntroductionMacro", ConfigKeyIntroductionMacroFunction, "this is a description of the macro."),
                        new Macro("helloBuildContextMacroResultMacro", HelloBuildContextMacroResultMacro),
                        Macro.Valued("helloMacro", HelloMacro),
                        Macro.Valued("helloBuildContextMacro", HelloBuildContextMacro));
  }

  private static readonly ConfigKey<string> IntroducedConfig = Key.Define("introducedConfig");

  private static Settings ConfigKeyIntroductionMacroFunction(Settings settings, string[] commandLineArguments) {
    return settings.AddGlobally(IntroducedConfig.Init("Foo bar value."));
  }

  private static string HelloMacro(Settings settings, string[] commandLineArguments) {
    return "Hello, Macro World!";
  }

  private static string HelloBuildContextMacro(BuildContext buildContext, string[] commandLineArguments) {
    return "Hello, BuildContext Macro World!";
  }

  private static MacroResult HelloBuildContextMacroResultMacro(BuildContext buildContext, string[] commandLineArguments) {
    return new MacroResult("Hello, BuildContext and MacroResult World!", buildContext);
  }
}
