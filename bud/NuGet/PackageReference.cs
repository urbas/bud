using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
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

    protected bool Equals(PackageReference other) {
      return Framework.Equals(other.Framework)
             && string.Equals(Id, other.Id)
             && Version.Equals(other.Version);
    }

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

    public static bool operator ==(PackageReference left, PackageReference right) => Equals(left, right);
    public static bool operator !=(PackageReference left, PackageReference right) => !Equals(left, right);

    public override string ToString()
      => $"PackageReference({Id}, {Version}, {Framework})";

    public static string GetHash(IEnumerable<PackageReference> packageReferences) {
      using (var memoryStream = new MemoryStream()) {
        using (var writer = new StreamWriter(memoryStream)) {
          WritePackagesConfigXml(packageReferences, writer);
          writer.Flush();
          memoryStream.Seek(0, SeekOrigin.Begin);
          return Hash(memoryStream);
        }
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

    private static string Hash(Stream memoryStream) {
      using (var digest = MD5.Create()) {
        return BitConverter.ToString(digest.ComputeHash(memoryStream));
      }
    }
  }
}