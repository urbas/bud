using System;
using System.IO;
using Bud.TempDir;
using NUnit.Framework;

namespace Bud.Scripting {
  [Category("IntegrationTest")]
  public class ScriptRunnerTest {
    [Test]
    public void RunScript_runs_the_script_in_the_current_directory() {
      using (var dir = new TemporaryDirectory()) {
        var outputDir = dir.CreateDir("output-dir");
        var fooExpected = dir.CreateFile("42 1337", "output-dir", "foo.expected");
        var script = dir.CreateFileFromResource("Bud.Scripting.TestScripts.CreateFooFile.cs",
                                                "CreateFooFile.cs");
        ScriptRunner.Run(script, new[] {"1337"}, outputDir);
        FileAssert.AreEqual(fooExpected, Path.Combine(outputDir, "foo"));
      }
    }

    [Test]
    public void RunScript_shows_an_informative_exception_message_on_compiler_error() {
      using (var dir = new TemporaryDirectory()) {
        var script = dir.CreateFileFromResource("Bud.Scripting.TestScripts.UnknownId.cs",
                                                "UnknownId.cs");
        var exception = Assert.Throws<Exception>(() => ScriptRunner.Run(script, new[] {""}, dir.Path));
        Assert.That(exception.Message, Contains.Substring("Blargh"));
      }
    }
  }
}