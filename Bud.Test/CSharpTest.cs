using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Bud.Compilation;
using Bud.Pipeline;
using NUnit.Framework;
using static Bud.Build;
using static Bud.CSharp;

namespace Bud {
  public class CSharpTest {
    private static readonly Configs SimpleCSharpProject = CSharpProject()
      .Modify(CSharpCompiler, TimedEmittingCompiler.Create);

    private static readonly Configs BudProject = "bud" / Project(@"../../../Bud").Add(SimpleCSharpProject, BudDependencies());
    private static readonly Configs BudTestProject = "budTest" / Project(@"../../../Bud.Test").Add(SimpleCSharpProject, BudTestDependencies());

    [Test]
    [Ignore]
    public async void Compiles_bud() {
      await BudProject.Add(BudTestProject)
                      .Modify("budTest" / Dependencies, AddBudReference)
                      .Get("budTest" / CSharp.Compilation)
                      .ToTask();
    }

    private IObservable<IEnumerable<Timestamped<Dependency>>> AddBudReference(IConfigs configs, IObservable<IEnumerable<Timestamped<Dependency>>> previousReferences) {
      var budCompilation = ("bud" / CSharp.Compilation)[configs];
      var budProjectId = ("bud" / ProjectId)[configs];
      var budReference = budCompilation.Select(result => new[] {new Timestamped<Dependency>(new Dependency(budProjectId, result.Compilation.ToMetadataReference()), DateTimeOffset.Now)});
      return previousReferences.JoinPipes(budReference);
    }

    private static Configs BudDependencies()
      => Configs.Empty.Set(Dependencies, c => FilesObservatory[c].ObserveAssemblies(
        Path.Combine(ProjectDir[c], "../packages/Microsoft.CodeAnalysis.Common.1.1.0-beta1-20150812-01/lib/net45/Microsoft.CodeAnalysis.dll"),
        Path.Combine(ProjectDir[c], "../packages/Microsoft.CodeAnalysis.CSharp.1.1.0-beta1-20150812-01/lib/net45/Microsoft.CodeAnalysis.CSharp.dll"),
        Path.Combine(ProjectDir[c], "../packages/Microsoft.Web.Xdt.2.1.0/lib/net40/Microsoft.Web.XmlTransform.dll"),
        Path.Combine(ProjectDir[c], "../packages/NuGet.Core.2.8.6/lib/net40-Client/NuGet.Core.dll"),
        Path.Combine(ProjectDir[c], "../packages/System.Collections.Immutable.1.1.38-beta-23225/lib/dotnet/System.Collections.Immutable.dll"),
        Path.Combine(ProjectDir[c], "../packages/Rx-Core.2.2.5/lib/net45/System.Reactive.Core.dll"),
        Path.Combine(ProjectDir[c], "../packages/Rx-Interfaces.2.2.5/lib/net45/System.Reactive.Interfaces.dll"),
        Path.Combine(ProjectDir[c], "../packages/Rx-Linq.2.2.5/lib/net45/System.Reactive.Linq.dll"),
        Path.Combine(ProjectDir[c], "../packages/Rx-PlatformServices.2.2.5/lib/net45/System.Reactive.PlatformServices.dll"),
        Path.Combine(ProjectDir[c], "../packages/System.Reflection.Metadata.1.1.0-alpha-00009/lib/dotnet/System.Reflection.Metadata.dll"),
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Collections.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Diagnostics.Debug.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Globalization.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.IO.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Linq.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Reflection.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Reflection.Extensions.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Reflection.Primitives.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Resources.ResourceManager.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Runtime.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Runtime.Extensions.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Runtime.InteropServices.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Text.Encoding.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Text.Encoding.Extensions.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Threading.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Threading.Tasks.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/mscorlib.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/System.dll",
        "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/System.Core.dll"));

    private static Configs BudTestDependencies()
      => BudDependencies().Modify(Dependencies, (c, references) => references.JoinPipes(FilesObservatory[c].ObserveAssemblies(
        Path.Combine(ProjectDir[c], "../packages/NUnit.2.6.4/lib/nunit.framework.dll"),
        Path.Combine(ProjectDir[c], "../packages/Moq.4.2.1507.0118/lib/net40/Moq.dll"))));
  }
}