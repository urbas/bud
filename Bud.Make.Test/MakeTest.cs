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
                     Make.Rule("foo.out", "foo.in", RemoveSpaces));
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
                     Make.Rule("foo.out", "foo.in", recipeMock.Object));
        recipeMock.Verify(s => s(It.IsAny<string>(), It.IsAny<string>()),
                          Times.Never);
      }
    }

    [Test]
    public void Execute_throws_when_rule_does_not_exist() {
      var exception = Assert.Throws<Exception>(() => {
        Make.Execute(new[] {"invalid.out"}, "/foo/bar");
      });
      Assert.That(exception.Message, Does.Contain("'invalid.out'"));
    }

    [Test]
    public void Execute_invokes_dependent_recipe() {
      using (var dir = new TmpDir()) {
        dir.CreateFile("This is Sparta!", "foo");
        var expectedOutput = dir.CreateFile("THISISSPARTA!", "expected_output");
        Make.Execute(new[] {"foo.nospace.upper"},
                     dir.Path,
                     Make.Rule("foo.nospace.upper", "foo.nospace", Uppercase),
                     Make.Rule("foo.nospace", "foo", RemoveSpaces));
        FileAssert.AreEqual(expectedOutput, dir.CreatePath("foo.nospace.upper"));
      }
    }

    [Test]
    public void Execute_does_nothing_when_no_rules_specified() {
        var recipeMock = new Mock<Action<string, string>>();
        Make.Execute(new string[] {},
                     "/foo/bar",
                     Make.Rule("foo.out", "foo.in", recipeMock.Object));
        recipeMock.Verify(s => s(It.IsAny<string>(), It.IsAny<string>()),
                          Times.Never);
    }

    private static void RemoveSpaces(string inputFile, string outputFile) {
      var inputFileContent = File.ReadAllText(inputFile);
      var outputFileContent = inputFileContent.Replace(" ", "");
      File.WriteAllText(outputFile, outputFileContent);
    }

    private static void Uppercase(string inputFile, string outputFile) {
      var inputFileContent = File.ReadAllText(inputFile);
      var outputFileContent = inputFileContent.ToUpperInvariant();
      File.WriteAllText(outputFile, outputFileContent);
    }
  }
}