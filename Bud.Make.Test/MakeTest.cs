using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                     Make.Rule("foo.out", RemoveSpaces, "foo.in"));
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
                     Make.Rule("foo.out", recipeMock.Object, "foo.in"));
        recipeMock.Verify(s => s(It.IsAny<string>(), It.IsAny<string>()),
                          Times.Never);
      }
    }

    [Test]
    public void Execute_throws_when_given_duplicate_rules() {
      var exception = Assert.Throws<Exception>(() => {
        Make.Execute(new[] {"foo"},
                     "foo",
                     Make.Rule("foo", RemoveSpaces, "bar"),
                     Make.Rule("foo", RemoveSpaces, "moo"));
      });
      Assert.That(exception.Message, Does.Contain("'foo'"));
    }

    [Test]
    public void Execute_throws_when_rule_does_not_exist() {
      var exception = Assert.Throws<Exception>(() => {
        Make.Execute(new[] {"invalid.out"}, "/foo/bar");
      });
      Assert.That(exception.Message, Does.Contain("'invalid.out'"));
    }

    [Test]
    public void Execute_invokes_dependent_recipes() {
      using (var dir = new TmpDir()) {
        dir.CreateFile("foo bar", "foo");
        var expectedOutput = dir.CreateFile("FOO BAR and foobar", "expected_output");
        Make.Execute(new[] {"foo.joined"},
                     dir.Path,
                     Make.Rule("foo.upper", Uppercase, "foo"),
                     Make.Rule("foo.nospace", RemoveSpaces, "foo"),
                     Make.Rule("foo.joined", JoinWithAnd, "foo.upper", "foo.nospace"));
        FileAssert.AreEqual(expectedOutput, dir.CreatePath("foo.joined"));
      }
    }

    [Test]
    public void Execute_does_not_invoke_dependent_rules_twice() {
      var recipeMock = new Mock<Action<string, string>>();
      Make.Execute(new[] {"foo.out3"},
                   "/foo/bar",
                   Make.Rule("foo.out1", recipeMock.Object, "foo.in"),
                   Make.Rule("foo.out2", (string inFile, string outFile) => {}, "foo.out1"),
                   Make.Rule("foo.out3", (inFiles, outFile) => {}, "foo.out1", "foo.out2"));
      recipeMock.Verify(s => s(It.IsAny<string>(), It.IsAny<string>()),
                        Times.Once);
    }

    [Test]
    public void Execute_throws_when_there_is_a_cycle() {
      var recipeMock = new Mock<Action<string, string>>();
      var ex = Assert.Throws<Exception>(() => {
        Make.Execute(new[] {"foo.out2"},
                     "/foo/bar",
                     Make.Rule("foo.out1", recipeMock.Object, "foo.in1"),
                     Make.Rule("foo.out2", recipeMock.Object, "foo.in2"),
                     Make.Rule("foo.in1", recipeMock.Object, "foo.out2"),
                     Make.Rule("foo.in2", recipeMock.Object, "foo.out1"));
      });
      Assert.That(ex.Message,
                  Does.Contain("'foo.out2 -> foo.in1 -> foo.out1 -> foo.in2 -> foo.out2'"));
    }

    [Test]
    public void Execute_does_nothing_when_no_rules_specified() {
      var recipeMock = new Mock<Action<string, string>>();
      Make.Execute(new string[] {},
                   "/foo/bar",
                   Make.Rule("foo.out", recipeMock.Object, "foo.in"));
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

    private static void JoinWithAnd(IReadOnlyList<string> inputFiles, string outputFile) {
      var inputFilesContent = inputFiles.Select(File.ReadAllText);
      var outputFileContent = string.Join(" and ", inputFilesContent);
      File.WriteAllText(outputFile, outputFileContent);
    }
  }
}