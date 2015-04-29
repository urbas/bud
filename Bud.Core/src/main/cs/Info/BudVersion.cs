using System.IO;

namespace Bud.Info {
  public static class BudVersion {
    public static readonly string Current = ReadVersionFromResourceFile();

    private static string ReadVersionFromResourceFile() {
      using (var versionFileStream = typeof(BudVersion).Assembly.GetManifestResourceStream("Bud.version")) {
        using (var versionFileReader = new StreamReader(versionFileStream)) {
          return versionFileReader.ReadToEnd().Trim();
        }
      }
    }
  }
}