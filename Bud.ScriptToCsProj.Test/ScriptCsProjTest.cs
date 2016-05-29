using System;
using System.IO;
using System.Reflection;
using Bud.Scripting;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Bud.ScriptToCsProj {
  public class ScriptCsProjTest {
    [Test]
    public void Generate_full_csproj()
      => AreEqual(GetResourceContent("Bud.ScriptToCsProj.BudScript.csproj"),
                  ScriptCsProj.BudScriptCsProj(new[] {
                    new Reference("Bud.Option", "Blah.dll"),
                    new Reference("System"),
                    new Reference("System.Core"),
                  }, "/foo/bar"));

    public static string GetResourceContent(string embeddedResourceName) {
      var manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourceName);
      if (manifestResourceStream == null) {
        throw new Exception($"Could not find the embedded resource '{embeddedResourceName}'.");
      }
      using (var expectedContent = manifestResourceStream) {
        using (var streamReader = new StreamReader(expectedContent)) {
          return streamReader.ReadToEnd();
        }
      }
    }
  }
}