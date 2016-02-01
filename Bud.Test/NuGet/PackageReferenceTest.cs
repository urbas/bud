using NuGet.Frameworks;
using NuGet.Versioning;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Bud.NuGet {
  public class PackageReferenceTest {
    [Test]
    public void Digest_produces_the_same_string_for_the_same_references()
      => AreEqual(PackageReference.GetHash(new[] {PackageA()}),
                  PackageReference.GetHash(new[] {PackageA()}));

    [Test]
    public void Digest_produces_different_strings_for_the_different_references()
      => AreNotEqual(PackageReference.GetHash(new[] {PackageA()}),
                     PackageReference.GetHash(new[] {PackageB()}));

    private static PackageReference PackageA()
      => new PackageReference("A",
                              NuGetVersion.Parse("1.2.3"),
                              NuGetFramework.Parse("net40"));

    private static PackageReference PackageB()
      => new PackageReference("B",
                              NuGetVersion.Parse("1.2.3"),
                              NuGetFramework.Parse("net40"));
  }
}