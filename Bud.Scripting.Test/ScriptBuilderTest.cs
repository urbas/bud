using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.TempDir;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using NUnit.Framework;

namespace Bud.Scripting {
  public class ScriptBuilderTest {
    [Test]
    public void Generate_throws_when_unknown_reference_specified() {
      using (var dir = new TemporaryDirectory()) {
        var script = dir.CreateFile(@"//!reference INVALIDREFERENCE
public class A {public static void Main(){}}", "Build.cs");
        var exception = Assert.Throws<Exception>(() => new ScriptBuilder(ImmutableDictionary<string, string>.Empty,
           new TestCSharpScriptCompiler()).Generate(dir.Path, ImmutableList.Create(script)));
        Assert.That(exception.Message,
                    Does.Contain("INVALIDREFERENCE"));
      }
    }

    [Test]
    public void Generate_copies_referenced_nonframework_assemblies_to_the_output_directory() {
      using (var dir = new TemporaryDirectory()) {
        var script = dir.CreateFile(@"//!reference A", "Build.cs");
        var assemblyA = dir.CreateFile("foo", "A.dll");
        var outputDir = dir.CreateDir("output");

        var references = ImmutableDictionary<string, string>.Empty.Add("A", assemblyA);

        new ScriptBuilder(references, new TestCSharpScriptCompiler())
          .Generate(outputDir, ImmutableList.Create(script));

        FileAssert.AreEqual(assemblyA,
                            Path.Combine(outputDir, "A.dll"));
      }
    }
  }

  public class TestCSharpScriptCompiler : ICSharpScriptCompiler {
    public IImmutableList<Diagnostic> Compile(IEnumerable<string> inputFiles, IEnumerable<MetadataReference> references, string outputExe)
      => ImmutableList<Diagnostic>.Empty;
  }
}