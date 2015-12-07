using Bud;
using Bud.Configuration.ApiV1;
using Bud.Cs;
using Bud.IO;
using static System.IO.Path;
using static Bud.Builds;
using static Bud.CSharp;

public class BudBuild : IBuild {
  public Conf Init()
    => Conf.Group(CSharpProject("Bud")
                    .Add(BudAssemblyReferences())
                    .Add(PackageDependencies, BudDependencies()),
                  CSharpProject("Bud.Test")
                    .Add(BudTestAssemblyReferences())
                    .Add(Dependencies, "../Bud")
                    .Add(PackageDependencies, BudTestDependencies()));

  private static Package[] BudDependencies() {
    return new[] {
      new Package("Microsoft.CodeAnalysis.Analyzers", "1.1.0-beta1-20150812-01", "net46"),
      new Package("Microsoft.CodeAnalysis.Common", "1.1.0-beta1-20150812-01", "net46"),
      new Package("Microsoft.CodeAnalysis.CSharp", "1.1.0-beta1-20150812-01", "net46"),
      new Package("Microsoft.Web.Xdt", "2.1.0", "net46"),
      new Package("NuGet.Core", "2.8.6", "net46"),
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
  }

  private static Package[] BudTestDependencies() {
    return new[] {
      new Package("Moq", "4.2.1507.0118", "net46"),
      new Package("NUnit", "2.6.4", "net46"),
      new Package("Rx-Testing", "2.2.5", "net46"),
    };
  }

  private static Conf BudAssemblyReferences()
    => AssemblyReferences.Set(c => new Files(Combine(ProjectDir[c], "../packages/Microsoft.CodeAnalysis.Common.1.1.0-beta1-20150812-01/lib/net45/Microsoft.CodeAnalysis.dll"), Combine(ProjectDir[c], "../packages/Microsoft.CodeAnalysis.CSharp.1.1.0-beta1-20150812-01/lib/net45/Microsoft.CodeAnalysis.CSharp.dll"), Combine(ProjectDir[c], "../packages/Microsoft.Web.Xdt.2.1.0/lib/net40/Microsoft.Web.XmlTransform.dll"), Combine(ProjectDir[c], "../packages/NuGet.Core.2.8.6/lib/net40-Client/NuGet.Core.dll"), Combine(ProjectDir[c], "../packages/System.Collections.Immutable.1.1.38-beta-23225/lib/dotnet/System.Collections.Immutable.dll"), Combine(ProjectDir[c], "../packages/Rx-Core.2.2.5/lib/net45/System.Reactive.Core.dll"), Combine(ProjectDir[c], "../packages/Rx-Interfaces.2.2.5/lib/net45/System.Reactive.Interfaces.dll"), Combine(ProjectDir[c], "../packages/Rx-Linq.2.2.5/lib/net45/System.Reactive.Linq.dll"), Combine(ProjectDir[c], "../packages/Rx-PlatformServices.2.2.5/lib/net45/System.Reactive.PlatformServices.dll"), Combine(ProjectDir[c], "../packages/System.Reflection.Metadata.1.1.0-alpha-00009/lib/dotnet/System.Reflection.Metadata.dll"), "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Collections.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Diagnostics.Debug.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Globalization.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.IO.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Linq.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Reflection.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Reflection.Extensions.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Reflection.Primitives.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Resources.ResourceManager.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Runtime.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Runtime.Extensions.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Runtime.InteropServices.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Text.Encoding.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Text.Encoding.Extensions.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Threading.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Threading.Tasks.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/mscorlib.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/System.dll", "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/System.Core.dll"));

  private static Conf BudTestAssemblyReferences()
    => BudAssemblyReferences().Modify(AssemblyReferences, (c, references) => references.ExpandWith(new Files(Combine(ProjectDir[c], "../packages/NUnit.2.6.4/lib/nunit.framework.dll"), Combine(ProjectDir[c], "../packages/Moq.4.2.1507.0118/lib/net40/Moq.dll"), Combine(ProjectDir[c], "../packages/Rx-Testing.2.2.5/lib/net45/Microsoft.Reactive.Testing.dll"))));
}