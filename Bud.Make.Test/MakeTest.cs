using System;
using System.IO;
using Moq;
using NUnit.Framework;

namespace Bud.Make {
  public class MakeTest {
    [Test]
    public void Execute_invokes_the_recipe_when_output_file_not_present() {
      using (var dir = new TmpDir()) {
        dir.CreateFile("This is Sparta!", "foo.in");
        Make.Execute(new[] {"foo.out"},
                     dir.Path,
                     new[] {
                       Make.Rule("foo.out", "foo.in", RemoveSpaces),
                     });
        FileAssert.AreEqual(dir.CreateFile("ThisisSparta!", "expected_output"),
                            dir.CreatePath("foo.out"));
      }
    }

    [Test]
    public void Execute_does_not_invoke_the_recipe_when_output_file_is_newer() {
      using (var dir = new TmpDir()) {
        var recipeMock = new Mock<Action<string, string>>();
        var inputFile = dir.CreateEmptyFile("foo.in");
        var outputFile = dir.CreateEmptyFile("foo.out");
        File.SetLastWriteTimeUtc(inputFile, File.GetLastWriteTimeUtc(outputFile) - TimeSpan.FromSeconds(5));
        Make.Execute(new[] {"foo.out"},
                     dir.Path,
                     new[] {
                       Make.Rule("foo.out", "foo.in", recipeMock.Object),
                     });
        recipeMock.Verify(s => s(It.IsAny<string>(), It.IsAny<string>()),
                          Times.Never);
      }
    }

    private static void RemoveSpaces(string inputFile, string outputFile) {
      var inputFileContent = File.ReadAllText(inputFile);
      var outputFileContent = inputFileContent.Replace(" ", "");
      File.WriteAllText(outputFile, outputFileContent);
    }
  }
}