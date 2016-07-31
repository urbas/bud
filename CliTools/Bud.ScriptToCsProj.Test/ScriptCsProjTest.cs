using System;
using System.IO;
using Bud.References;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Bud.ScriptToCsProj {
  public class ScriptCsProjTest {
    [Test]
    public void BudScriptCsProj_procudes_expected_string()
      => AreEqual(GetResourceContent("Bud.ScriptToCsProj.BudScript.csproj"),
                  ScriptCsProj.BudScriptCsProj(new[] {new Assembly("Bud.Option", "Blah.dll")},
                                               new[] {
                                                 new FrameworkAssembly("System", FrameworkAssembly.MaxVersion),
                                                 new FrameworkAssembly("System.Core", FrameworkAssembly.MaxVersion),
                                               }, "/foo/bar"));

    public static string GetResourceContent(string embeddedResourceName) {
      var manifestResourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourceName);
      if (manifestResourceStream == null) {
        throw new Exception($"Could not find the embedded resource '{embeddedResourceName}'.");
      }
      using (var expectedContent = manifestResourceStream) {
        using (var streamReader = new StreamReader(expectedContent)) {
          return streamReader.ReadToEnd().Replace("\r\n", "\n");
        }
      }
    }
  }
}