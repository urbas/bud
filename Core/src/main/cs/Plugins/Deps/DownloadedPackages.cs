using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Plugins.Deps {
  public class DownloadedPackages {
    public readonly List<DownloadedPackage> InstalledVersions;

    private DownloadedPackages(List<DownloadedPackage> downloadedPackageInfos) {
      InstalledVersions = downloadedPackageInfos;
      InstalledVersions.Sort();
    }

    public DownloadedPackages(IEnumerable<IPackage> downloadedPackages) : this(downloadedPackages.Select(package => new DownloadedPackage(package)).ToList()) {}

    public DownloadedPackage GetBestSuitedVersion(SemanticVersion lowerBoundVersion) {
      if (InstalledVersions.Count > 0 && InstalledVersions[0].Version >= lowerBoundVersion) {
        return InstalledVersions[0];
      }
      return null;
    }

    public DownloadedPackage GetMostCurrentVersion() {
      return InstalledVersions.Count == 0 ? null : InstalledVersions[0];
    }

    public void ToJson(JsonTextWriter jsonWriter) {
      jsonWriter.WriteStartArray();
      foreach (var downloadedPackage in InstalledVersions) {
        downloadedPackage.ToJson(jsonWriter);
      }
      jsonWriter.WriteEndArray();
    }

    public static DownloadedPackages FromJson(JsonReader jsonReader) {
      var downloadedPackages = new List<DownloadedPackage>();
      jsonReader.Read();
      while (jsonReader.Read() && jsonReader.TokenType == JsonToken.StartArray) {
        downloadedPackages.Add(DownloadedPackage.FromJson(jsonReader));
        jsonReader.Read();
      }
      return new DownloadedPackages(downloadedPackages);
    }
  }

  public class DownloadedPackagesJsonConverter {}
}