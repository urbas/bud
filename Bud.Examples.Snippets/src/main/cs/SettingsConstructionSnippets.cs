namespace Bud.Examples.Snippets {
  public class SettingsConstructionSnippets {
    public static Settings ChainedSettingsConstruction(Settings settings, Setup settingFoo, Setup settingBar, Setup settingBaz) {
      // SNIPPET: chainedSettingsConstruction
      return settings.Add(settingFoo)
                     .Add(settingBar)
                     .Add(settingBaz);
      // END_SNIPPET: chainedSettingsConstruction
    }
  }
}