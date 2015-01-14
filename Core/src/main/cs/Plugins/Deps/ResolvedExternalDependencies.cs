using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Plugins.Deps {
  public class ResolvedExternalDependencies {
    public readonly IDictionary<string, DownloadedPackages> FetchedPackages;

    public ResolvedExternalDependencies(IDictionary<string, DownloadedPackages> fetchedPackages) {
      FetchedPackages = fetchedPackages;
    }

    public ResolvedExternalDependencies(IEnumerable<IGrouping<string, IPackage>> fetchedPackages) : this(ToJsonSerializable(fetchedPackages)) {}

    private static IDictionary<string, DownloadedPackages> ToJsonSerializable(IEnumerable<IGrouping<string, IPackage>> fetchedPackages) {
      return fetchedPackages.ToDictionary(packageGroup => packageGroup.Key,
                                          packageGroup => new DownloadedPackages(packageGroup));
    }

    public ResolvedExternalDependency GetResolvedNuGetDependency(ExternalDependency dependency) {
      if (dependency.Version == null) {
        var mostCurrentVersion = FetchedPackages[dependency.Id].GetMostCurrentVersion();
        if (mostCurrentVersion != null) {
          return new ResolvedExternalDependency(dependency, mostCurrentVersion);
        }
        throw new Exception(string.Format("Could not find any version of the package '{0}'. Try running '{1}' to download packages.", dependency.Id, DependenciesKeys.Fetch));
      }

      var bestSuitedVersion = FetchedPackages[dependency.Id].GetBestSuitedVersion(dependency.Version);
      if (bestSuitedVersion != null) {
        return new ResolvedExternalDependency(dependency, bestSuitedVersion);
      }
      throw new Exception(string.Format("Could not find the version '{0}' of package '{1}'. Try running '{2}' to download packages.", dependency.Id, dependency.Version, DependenciesKeys.Fetch));
    }

    public void ToJson(JsonTextWriter jsonWriter) {
      jsonWriter.WriteStartObject();
      foreach (var packageId2Package in FetchedPackages) {
        jsonWriter.WritePropertyName(packageId2Package.Key);
        packageId2Package.Value.ToJson(jsonWriter);
      }
      jsonWriter.WriteEndObject();
    }

    public static ResolvedExternalDependencies FromJson(JsonReader jsonReader) {
      var downloadedPackages = new Dictionary<string, DownloadedPackages>();
      if (jsonReader.Read() && jsonReader.TokenType == JsonToken.StartObject) {
        while (jsonReader.Read() && jsonReader.TokenType != JsonToken.EndObject) {
          var packageId = (string)jsonReader.Value;
          var installedVersions = DownloadedPackages.FromJson(jsonReader);
          downloadedPackages.Add(packageId, installedVersions);
        }
      }
      return new ResolvedExternalDependencies(downloadedPackages);
    }
  }
}