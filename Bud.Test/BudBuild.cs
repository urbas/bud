using Bud;
using Bud.Configuration.ApiV1;
using Bud.Cs;
using static Bud.Builds;
using static Bud.CSharp;

public class BudBuild : IBuild {
  public Conf Init()
    => Conf.Group(CSharpProject("Bud")
                    .Add(PackageDependencies, BudDependencies),
                  CSharpProject("Bud.Test")
                    .Add(Dependencies, "../Bud")
                    .Add(PackageDependencies, BudTestDependencies));

  private static Package[] BudDependencies { get; } = {
    new Package("Microsoft.CodeAnalysis.Analyzers", "1.1.0-beta1-20150812-01", "net46"),
    new Package("Microsoft.CodeAnalysis.Common", "1.1.0-beta1-20150812-01", "net46"),
    new Package("Microsoft.CodeAnalysis.CSharp", "1.1.0-beta1-20150812-01", "net46"),
    new Package("Microsoft.Web.Xdt", "2.1.0", "net46"),
    new Package("Rx-Core", "2.2.5", "net46"),
    new Package("Rx-Interfaces", "2.2.5", "net46"),
    new Package("Rx-Linq", "2.2.5", "net46"),
    new Package("Rx-Main", "2.2.5", "net46"),
    new Package("Rx-PlatformServices", "2.2.5", "net46"),
    new Package("System.Collections", "4.0.0", "net46"),
    new Package("System.Collections.Immutable", "1.1.38-beta-23225", "net46"),
    new Package("System.Diagnostics.Debug", "4.0.0", "net46"),
    new Package("System.Globalization", "4.0.0", "net46"),
    new Package("System.IO", "4.0.0", "net46"),
    new Package("System.Linq", "4.0.0", "net46"),
    new Package("System.Reflection", "4.0.0", "net46"),
    new Package("System.Reflection.Extensions", "4.0.0", "net46"),
    new Package("System.Reflection.Metadata", "1.1.0-alpha-00009", "net46"),
    new Package("System.Reflection.Primitives", "4.0.0", "net46"),
    new Package("System.Resources.ResourceManager", "4.0.0", "net46"),
    new Package("System.Runtime", "4.0.0", "net46"),
    new Package("System.Runtime.Extensions", "4.0.0", "net46"),
    new Package("System.Runtime.InteropServices", "4.0.0", "net46"),
    new Package("System.Text.Encoding", "4.0.0", "net46"),
    new Package("System.Text.Encoding.Extensions", "4.0.0", "net46"),
    new Package("System.Threading", "4.0.0", "net46")
  };

  private static Package[] BudTestDependencies { get; } = {
    new Package("Moq", "4.2.1507.0118", "net46"),
    new Package("NUnit", "2.6.4", "net46"),
    new Package("Rx-Testing", "2.2.5", "net46")
  };
}