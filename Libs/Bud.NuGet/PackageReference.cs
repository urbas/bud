using System;
using System.Collections.Generic;
using System.IO;
using NuGet.Frameworks;
using NuGet.Versioning;

namespace Bud.NuGet {
  public class PackageReference {
    public string Id { get; }
    public NuGetVersion Version { get; }
    public NuGetFramework Framework { get; }

    public PackageReference(string id, NuGetVersion version, NuGetFramework framework) {
      Id = id;
      Version = version;
      Framework = framework;
    }

    protected bool Equals(PackageReference other)
      => Framework.Equals(other.Framework)
         && String.Equals(Id, other.Id)
         && Version.Equals(other.Version);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == GetType() && Equals((PackageReference) obj);
    }

    public override int GetHashCode() {
      unchecked {
        var hashCode = Framework.GetHashCode();
        hashCode = (hashCode*397) ^ Id.GetHashCode();
        hashCode = (hashCode*397) ^ Version.GetHashCode();
        return hashCode;
      }
    }

    public static void WritePackagesConfigXml(IEnumerable<PackageReference> packageReferences,
                                              TextWriter writer) {
      writer.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
      writer.Write("<packages>\n");
      foreach (var packageReference in packageReferences) {
        writer.Write("  <package" +
                     $" id=\"{packageReference.Id}\"" +
                     $" version=\"{packageReference.Version}\"" +
                     $" targetFramework=\"{packageReference.Framework.GetShortFolderName()}\" />\n");
      }
      writer.Write("</packages>");
    }

    public static string WritePackagesConfigXml(IEnumerable<PackageReference> packageReferences, string packageConfigFile) {
      using (var packagesConfigFileStream = new FileStream(packageConfigFile, FileMode.Create, FileAccess.Write)) {
        using (var packageConfigFileWriter = new StreamWriter(packagesConfigFileStream)) {
          WritePackagesConfigXml(packageReferences,
                                                  packageConfigFileWriter);
        }
      }
      return packageConfigFile;
    }
  }
}