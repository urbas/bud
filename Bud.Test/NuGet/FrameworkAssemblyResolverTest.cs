using NuGet.Frameworks;
using NUnit.Framework;
using static System.IO.Path;
using static Bud.NuGet.FrameworkAssemblyResolver;
using static NUnit.Framework.Assert;

namespace Bud.NuGet {
  public class FrameworkAssemblyResolverTest {
    private static readonly NuGetFramework Net46 = NuGetFramework.Parse("net46");
    private static readonly NuGetFramework Net45 = NuGetFramework.Parse("net45");
    private static readonly NuGetFramework Net452 = NuGetFramework.Parse("net452");

    [Test]
    public void Returns_empty_list_when_given_no_references()
      => IsEmpty(ResolveFrameworkAsseblies(new FrameworkAssemblyReference[] {}));

    [Test]
    public void Returns_the_facade_assembly_path_for_net46()
      => AreEqual(new[] {Combine(ReferenceAssembliesPath, "v4.6", "Facades", "System.Runtime.dll")},
                  ResolveFrameworkAsseblies(new[] {new FrameworkAssemblyReference("System.Runtime", Net46)}));

    [Test]
    public void Returns_the_facade_assembly_path_for_net45()
      => AreEqual(new[] {Combine(ReferenceAssembliesPath, "v4.5", "Facades", "System.Runtime.dll")},
                  ResolveFrameworkAsseblies(new[] {new FrameworkAssemblyReference("System.Runtime", Net45)}));

    [Test]
    public void Returns_the_facade_assembly_path_for_net452()
      => AreEqual(new[] {Combine(ReferenceAssembliesPath, "v4.5.2", "Facades", "System.Runtime.dll")},
                  ResolveFrameworkAsseblies(new[] {new FrameworkAssemblyReference("System.Runtime", Net452)}));

    [Test]
    public void Returns_the_path_for_non_facade_assemblies()
      => AreEqual(new[] {Combine(ReferenceAssembliesPath, "v4.6", "System.Configuration.dll")},
                  ResolveFrameworkAsseblies(new[] {new FrameworkAssemblyReference("System.Configuration", Net46)}));
  }
}