using System.IO;
using Bud.Cs;
using Bud.IO;
using Bud.TempDir;
using NUnit.Framework;
using static System.IO.Path;
using static Bud.Cli.BuildScriptCompilation;
using static NUnit.Framework.Assert;
using Contains = NUnit.Framework.Contains;

namespace Bud.Cli {
  [Category("IntegrationTest")]
  public class BuildScriptCompilationTest {
    private TemporaryDirectory tmpDir;

    [SetUp]
    public void SetUp() => tmpDir = new TemporaryDirectory();

    [TearDown]
    public void TearDown() => tmpDir.Dispose();

    [Test]
    public void CompileBuildScript_throws_FileNotFoundException_when_no_build_file_is_present() {
      var exception = Throws<FileNotFoundException>(() => InvokeCompileBuildScript());
      That(exception.Message, Contains.Substring("Build.cs"));
    }

    [Test]
    public void CompileBuildScript_reports_compilation_failure()
      => IsFalse(InvalidBuildScript().Success);

    [Test]
    public void CompileBuildScript_produces_a_compilation_error_message()
      => IsNotEmpty(InvalidBuildScript().Diagnostics);

    [Test]
    public void CompileBuildScript_places_the_Build_assembly_into_the_build_folder()
      => That(ValidBuildScript().AssemblyPath,
              Is.EqualTo(Combine(tmpDir.Path, "build", "build", "build.dll"))
                .And.Exist);

    private CompileOutput InvokeCompileBuildScript()
      => CompileBuildScript(tmpDir.Path, Combine(tmpDir.Path, "Build.cs"));

    private CompileOutput InvalidBuildScript() {
      tmpDir.CreateFile("not valid C#", "Build.cs");
      return InvokeCompileBuildScript();
    }

    private CompileOutput ValidBuildScript() {
      tmpDir.CreateFile(
        "using Bud.V1;\n" +
        "using static Bud.V1.Cs;\n" +
        "using static Bud.V1.Basic;\n" +
        "\n" +
        "public class SampleBuild : IBuild {\n" +
        "  public Conf Init()\n" +
        "    => Projects(CsApp(\"bud\")\n" +
        "                  .Set(ProjectVersion, \"0.5.0-pre-3\")\n" +
        "                  .Set(NuGetPublishing.ProjectUrl, \"https://github.com/urbas/bud\"),\n" +
        "                CsLib(\"Bud.Test\")\n" +
        "                  .Add(Dependencies, \"../bud\"));\n" +
        "}",
        "Build.cs");
      return InvokeCompileBuildScript();
    }
  }
}