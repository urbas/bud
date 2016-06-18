using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using static Bud.Make.Make;
using static NUnit.Framework.Assert;

namespace Bud.Make {
  public class MakeTest {
    [Test]
    public void Execute_invokes_the_recipe_when_output_file_not_present() {
      using (var dir = new TmpDir()) {
        dir.CreateFile("This is Sparta!", "foo.in");
        Execute(new[] {Rule("foo.out", RemoveSpaces, "foo.in")}, "foo.out", dir.Path);
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
        Execute(new[] {Rule("foo.out", recipeMock.Object, "foo.in")}, "foo.out", dir.Path);
        recipeMock.Verify(s => s(It.IsAny<string>(), It.IsAny<string>()),
                          Times.Never);
      }
    }

    [Test]
    public void Execute_throws_when_given_duplicate_rules() {
      var exception = Throws<Exception>(() => {
        Execute(new[] {Rule("foo", RemoveSpaces, "bar"), Rule("foo", RemoveSpaces, "moo")}, "foo");
      });
      That(exception.Message, Does.Contain("'foo'"));
    }

    [Test]
    public void Execute_throws_when_rule_does_not_exist() {
      var exception = Throws<Exception>(() => {
        Execute(new[] {Rule("out", RemoveSpaces, "in")}, "invalid.out", "/foo/bar");
      });
      That(exception.Message, Does.Contain("'invalid.out'"));
    }

    [Test]
    public void Execute_invokes_dependent_recipes() {
      using (var dir = new TmpDir()) {
        dir.CreateFile("foo bar", "foo");
        var expectedOutput = dir.CreateFile("FOO BAR and foobar", "expected_output");
        Execute(new[] {
          Rule("foo.upper", Uppercase, "foo"),
          Rule("foo.nospace", RemoveSpaces, "foo"),
          Rule("foo.joined", JoinWithAnd, "foo.upper", "foo.nospace"),
        }, "foo.joined", dir.Path);
        FileAssert.AreEqual(expectedOutput, dir.CreatePath("foo.joined"));
      }
    }

    [Test]
    public void Execute_does_not_invoke_dependent_rules_twice() {
      var recipeMock = new Mock<Action<string, string>>();
      Execute(new[] {
        Rule("foo.out1", recipeMock.Object, "foo.in"),
        Rule("foo.out2", (string inFile, string outFile) => {}, "foo.out1"),
        Rule("foo.out3", (inFiles, outFile) => {}, "foo.out1", "foo.out2") ,
      }, "foo.out3", "/foo/bar");
      recipeMock.Verify(s => s(It.IsAny<string>(), It.IsAny<string>()),
                        Times.Once);
    }

    [Test]
    public void Execute_throws_when_there_is_a_cycle() {
      var recipeMock = new Mock<Action<string, string>>();
      var ex = Throws<Exception>(() => {
        Execute(new[] {
          Rule("foo.out1", recipeMock.Object, "foo.in1"),
          Rule("foo.out2", recipeMock.Object, "foo.in2"),
          Rule("foo.in1", recipeMock.Object, "foo.out2"),
          Rule("foo.in2", recipeMock.Object, "foo.out1"),
        }, "foo.out2", "/foo/bar");
      });
      That(ex.Message,
           Does.Contain("'foo.out2 -> foo.in1 -> foo.out1 -> foo.in2 -> foo.out2'"));
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