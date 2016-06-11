using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.References;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bud.Scripting {
  public class ScriptBuilderTest {
    private TmpDir dir;

    [SetUp]
    public void SetUp() => dir = new TmpDir();

    [TearDown]
    public void TearDown() => dir.Dispose();

    [Test]
    public void Build_throws_when_unknown_reference_specified() {
      var script = dir.CreateFile(@"//!reference INVALIDREFERENCE
public class A {public static void Main(){}}", "Build.cs");
      var exception = Assert.Throws<Exception>(() => {
        ScriptBuilder.Build(ImmutableList.Create(script),
                            new TestReferenceResolver(),
                            new TestCSharpScriptCompiler(),
                            Path.Combine(dir.Path, "build-script.exe"));
      });
      Assert.That(exception.Message,
                  Does.Contain("INVALIDREFERENCE"));
    }

    [Test]
    public void Build_copies_referenced_nonframework_assemblies_to_the_output_directory() {
      var script = dir.CreateFile(@"//!reference A", "Build.cs");
      var assemblyA = dir.CreateFile("foo", "A.dll");
      var outputDir = dir.CreateDir("output");

      ScriptBuilder.Build(ImmutableList.Create(script),
                          new TestReferenceResolver(ImmutableDictionary<string, string>.Empty.Add("A", assemblyA)),
                          new TestCSharpScriptCompiler(),
                          Path.Combine(outputDir, "build-script.exe"));

      FileAssert.AreEqual(assemblyA,
                          Path.Combine(outputDir, "A.dll"));
    }

    [Test]
    public void Build_returns_resolved_assembly_references() {
      var script = dir.CreateFile(@"//!reference A", "Build.cs");
      var assemblyA = dir.CreateFile("foo", "A.dll");
      var outputDir = dir.CreateDir("output");

      var builtScript = ScriptBuilder.Build(ImmutableList.Create(script),
                                            new TestReferenceResolver(ImmutableDictionary<string, string>.Empty.Add("A", assemblyA)),
                                            new TestCSharpScriptCompiler(),
                                            Path.Combine(outputDir, "build-script.exe"));

      Assert.That(builtScript.ResolvedScriptReferences.AssemblyReferences,
                  Is.EquivalentTo(ImmutableDictionary<string, string>.Empty.Add("A", assemblyA)));
    }

    [Test]
    public void Build_returns_resolved_framework_assembly_references() {
      var script = dir.CreateFile(@"//!reference System.Core", "Build.cs");
      var outputDir = dir.CreateDir("output");

      var builtScript = ScriptBuilder.Build(ImmutableList.Create(script),
                                            new TestReferenceResolver(ImmutableDictionary<string, string>.Empty),
                                            new TestCSharpScriptCompiler(),
                                            Path.Combine(outputDir, "build-script.exe"));

      var expectedAssemblyPath = WindowsFrameworkReferenceResolver.ResolveFrameworkAssembly("System.Core", ScriptBuilder.MaxVersion).Value;
      Assert.That(builtScript.ResolvedScriptReferences.FrameworkAssemblyReferences,
                  Is.EquivalentTo(ImmutableDictionary<string, string>.Empty.Add("System.Core", expectedAssemblyPath)));
    }

    [Test]
    public void Build_produces_the_packages_config_file() {
      var script = dir.CreateFile(@"//!nuget Foo 1.2.3", "Build.cs");
      var expectedPackagesFile = dir.CreateFile(
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
        "<packages>\n" +
        "  <package id=\"Foo\" version=\"1.2.3\" targetFramework=\"any\" />\n" +
        "</packages>", "expected.packages.config");
      var outputDir = dir.CreateDir("output");

      ScriptBuilder.Build(ImmutableList.Create(script),
                          new TestReferenceResolver(ImmutableDictionary<string, string>.Empty),
                          new TestCSharpScriptCompiler(),
                          Path.Combine(outputDir, "build-script.exe"));

      FileAssert.AreEqual(expectedPackagesFile,
                          Path.Combine(outputDir, "packages.config"));
    }
  }

  public class TestReferenceResolver : IReferenceResolver {
    private readonly IReadOnlyDictionary<string, string> knownReferences;

    public TestReferenceResolver(IReadOnlyDictionary<string, string> references = null) {
      knownReferences = references ?? ImmutableDictionary<string, string>.Empty;
    }

    public IDictionary<string, Option<string>> Resolve(IEnumerable<string> references)
      => references.Aggregate(ImmutableDictionary<string, Option<string>>.Empty,
                              (resolvedReferences, reference) => resolvedReferences.Add(reference, knownReferences.Get(reference)));
  }

  public class TestCSharpScriptCompiler : ICSharpScriptCompiler {
    public IImmutableList<Diagnostic> Compile(IEnumerable<string> inputFiles,
                                              IEnumerable<MetadataReference> references,
                                              string outputExe)
      => ImmutableList<Diagnostic>.Empty;
  }
}