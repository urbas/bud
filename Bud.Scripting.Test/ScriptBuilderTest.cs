using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Bud.FrameworkAssemblies;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bud.Scripting {
  public class ScriptBuilderTest {
    [Test]
    public void Build_throws_when_unknown_reference_specified() {
      using (var dir = new TmpDir()) {
        var script = dir.CreateFile(@"//!reference INVALIDREFERENCE
public class A {public static void Main(){}}", "Build.cs");
        var exception = Assert.Throws<Exception>(() => {
          ScriptBuilder.Build(ImmutableList.Create(script),
                              ImmutableDictionary<string, string>.Empty,
                              new TestCSharpScriptCompiler(),
                              Path.Combine(dir.Path, "build-script.exe"));
        });
        Assert.That(exception.Message,
                    Does.Contain("INVALIDREFERENCE"));
      }
    }

    [Test]
    public void Generate_copies_referenced_nonframework_assemblies_to_the_output_directory() {
      using (var dir = new TmpDir()) {
        var script = dir.CreateFile(@"//!reference A", "Build.cs");
        var assemblyA = dir.CreateFile("foo", "A.dll");
        var outputDir = dir.CreateDir("output");

        ScriptBuilder.Build(ImmutableList.Create(script),
                            ImmutableDictionary<string, string>.Empty.Add("A", assemblyA),
                            new TestCSharpScriptCompiler(),
                            Path.Combine(outputDir, "build-script.exe"));

        FileAssert.AreEqual(assemblyA,
                            Path.Combine(outputDir, "A.dll"));
      }
    }

    [Test]
    public void Generate_returns_resolved_assembly_references() {
      using (var dir = new TmpDir()) {
        var script = dir.CreateFile(@"//!reference A", "Build.cs");
        var assemblyA = dir.CreateFile("foo", "A.dll");
        var outputDir = dir.CreateDir("output");

        var builtScript = ScriptBuilder.Build(ImmutableList.Create(script),
                                              ImmutableDictionary<string, string>.Empty.Add("A", assemblyA),
                                              new TestCSharpScriptCompiler(),
                                              Path.Combine(outputDir, "build-script.exe"));

        Assert.That(builtScript.ResolvedReferences.AssemblyReferences,
                    Is.EquivalentTo(ImmutableDictionary<string, string>.Empty.Add("A", assemblyA)));
      }
    }

    [Test]
    public void Generate_returns_resolved_framework_assembly_references() {
      using (var dir = new TmpDir()) {
        var script = dir.CreateFile(@"//!reference System.Core", "Build.cs");
        var outputDir = dir.CreateDir("output");

        var builtScript = ScriptBuilder.Build(ImmutableList.Create(script),
                                              ImmutableDictionary<string, string>.Empty,
                                              new TestCSharpScriptCompiler(),
                                              Path.Combine(outputDir, "build-script.exe"));

        var expectedAssemblyPath = WindowsResolver.ResolveFrameworkAssembly("System.Core", ScriptBuilder.MaxVersion).Value;
        Assert.That(builtScript.ResolvedReferences.FrameworkAssemblyReferences,
                    Is.EquivalentTo(ImmutableDictionary<string, string>.Empty.Add("System.Core", expectedAssemblyPath)));
      }
    }
  }

  public class TestAssemblyPaths : IAssemblyPaths {
    private readonly IReadOnlyDictionary<string, string> references;

    public TestAssemblyPaths(IReadOnlyDictionary<string, string> references = null) {
      this.references = references ?? ImmutableDictionary<string, string>.Empty;
    }

    public IReadOnlyDictionary<string, string> Get() => references;
  }

  public class TestCSharpScriptCompiler : ICSharpScriptCompiler {
    public IImmutableList<Diagnostic> Compile(IEnumerable<string> inputFiles, IEnumerable<MetadataReference> references, string outputExe) => ImmutableList<Diagnostic>.Empty;
  }
}