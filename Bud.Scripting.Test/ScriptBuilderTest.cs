using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.NuGet;
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
    public void Build_copies_referenced_nonframework_assemblies_to_the_output_directory() {
      var script = dir.CreateFile(@"//!reference A", "Build.cs");
      var assemblyA = dir.CreateFile("foo", "A.dll");
      var outputDir = dir.CreateDir("output");

      ScriptBuilder.Build(ImmutableList.Create(script),
                          new TestReferenceResolver(ImmutableDictionary<string, string>.Empty.Add("A", assemblyA)),
                          new TestCSharpScriptCompiler(),
                          new TestNuGetReferenceResolver(),
                          Path.Combine(outputDir, "build-script.exe"));

      FileAssert.AreEqual(assemblyA,
                          Path.Combine(outputDir, "A.dll"));
    }

    [Test]
    public void Build_copies_nuget_references_to_the_output_directory() {
      var script = dir.CreateFile(@"//!nuget Foo 1.2.3", "Build.cs");
      var expectedPackagesFile = dir.CreateFile("blah", "Foo.dll");
      var outputDir = dir.CreateDir("output");

      ScriptBuilder.Build(ImmutableList.Create(script),
                          new TestReferenceResolver(ImmutableDictionary<string, string>.Empty),
                          new TestCSharpScriptCompiler(),
                          new TestNuGetReferenceResolver(new ResolvedReferences(new[] {new Assembly("Foo", expectedPackagesFile)}, new FrameworkAssembly[0])),
                          Path.Combine(outputDir, "build-script.exe"));

      FileAssert.AreEqual(expectedPackagesFile,
                          Path.Combine(outputDir, "Foo.dll"));
    }

    [Test]
    public void Build_returns_resolved_assembly_references() {
      var script = dir.CreateFile(@"//!reference A", "Build.cs");
      var assemblyA = dir.CreateFile("foo", "A.dll");
      var outputDir = dir.CreateDir("output");

      var builtScript = ScriptBuilder.Build(ImmutableList.Create(script),
                                            new TestReferenceResolver(ImmutableDictionary<string, string>.Empty.Add("A", assemblyA)),
                                            new TestCSharpScriptCompiler(),
                                            new TestNuGetReferenceResolver(),
                                            Path.Combine(outputDir, "build-script.exe"));

      Assert.That(builtScript.ResolvedScriptReferences.Assemblies,
                  Is.EquivalentTo(ImmutableList.Create(new Assembly("A", assemblyA))));
    }

    [Test]
    public void Build_returns_resolved_framework_assembly_references() {
      var script = dir.CreateFile(@"//!reference System.Core", "Build.cs");
      var outputDir = dir.CreateDir("output");

      var builtScript = ScriptBuilder.Build(ImmutableList.Create(script),
                                            new TestReferenceResolver(ImmutableDictionary<string, string>.Empty),
                                            new TestCSharpScriptCompiler(),
                                            new TestNuGetReferenceResolver(),
                                            Path.Combine(outputDir, "build-script.exe"));

      Assert.That(builtScript.ResolvedScriptReferences.FrameworkAssemblies,
                  Is.EquivalentTo(ImmutableList.Create(new FrameworkAssembly("System.Core", FrameworkAssembly.MaxVersion))));
    }
  }

  public class TestNuGetReferenceResolver : INuGetReferenceResolver {
    public ResolvedReferences ResolvedReferences { get; }

    public TestNuGetReferenceResolver(ResolvedReferences resolvedReferences = null) {
      ResolvedReferences = resolvedReferences ?? ResolvedReferences.Empty;
    }

    public ResolvedReferences Resolve(IEnumerable<PackageReference> packageReferences, string outputDir)
      => ResolvedReferences;
  }

  public class TestReferenceResolver : IReferenceResolver {
    private readonly IReadOnlyDictionary<string, string> knownReferences;

    public TestReferenceResolver(IReadOnlyDictionary<string, string> references = null) {
      knownReferences = references ?? ImmutableDictionary<string, string>.Empty;
    }

    ResolvedReferences IReferenceResolver.Resolve(IEnumerable<string> references) {
      var assemblyPaths = references.Aggregate(ImmutableDictionary<string, Option<string>>.Empty,
                                               (resolvedReferences, reference) => resolvedReferences.Add(reference, knownReferences.Get(reference)));
      return new ResolvedReferences(assemblyPaths.Where(pair => pair.Value.HasValue)
                                                 .Select(pair => new Assembly(pair.Key, pair.Value.Value)),
                                    assemblyPaths.Where(pair => !pair.Value.HasValue)
                                                 .Select(pair => new FrameworkAssembly(pair.Key, FrameworkAssembly.MaxVersion)));
    }
  }

  public class TestCSharpScriptCompiler : ICSharpScriptCompiler {
    public IImmutableList<Diagnostic> Compile(IEnumerable<string> inputFiles,
                                              ResolvedReferences references,
                                              string outputExe)
      => ImmutableList<Diagnostic>.Empty;
  }
}