using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using Bud.Cli;
using Bud.IO;
using Bud.NuGet;
using Bud.Util;
using Bud.V1;
using static Bud.V1.Api;

namespace Bud.Distribution {
  public class BinTrayDistribution {
    public static IObservable<bool> Distribute(IConf c)
      => Distribute(DistributionArchive[c],
                    ProjectId[c],
                    ProjectId[c],
                    Environment.UserName,
                    ProjectVersion[c],
                    BuildDir[c]);

    public static IObservable<bool> Distribute(IObservable<string> observedArchive,
                                               string repositoryId,
                                               string packadeId,
                                               string username,
                                               string packageVersion,
                                               string buildDir)
      => observedArchive.Select(
        archive => PushToBintray(archive, repositoryId, packadeId, packageVersion, username)
                     .Map(archiveUrl => PushToChoco(repositoryId, packadeId, packageVersion, archiveUrl, username, buildDir))
                     .GetOrElse(false));

    public static Option<string> PushToBintray(string package,
                                               string repositoryId,
                                               string packageId,
                                               string packageVersion,
                                               string username) {
      var packagePublishUrl = BintrayPublishPackageUrl(packageId, repositoryId, username, packageVersion);
      var credentials = LoadBintrayCredentials(username);
      using (var httpClient = new HttpClient()) {
        using (var content = new StreamContent(File.OpenRead(package))) {
          using (var request = new HttpRequestMessage(HttpMethod.Put, packagePublishUrl)) {
            request.Headers.Add("Authorization", $"Basic {credentials}");
            request.Content = content;
            request.Method = HttpMethod.Put;

            var responseTask = httpClient.SendAsync(request);
            responseTask.Wait();
            using (var response = responseTask.Result) {
              return response.StatusCode == HttpStatusCode.Created ?
                       BintrayArchiveDownloadUrl(repositoryId, packageId, username, packageVersion) :
                       Option.None<string>();
            }
          }
        }
      }
    }

    public static bool PushToChoco(string repositoryId, string packageId, string packageVersion, string archiveUrl, string username, string buildDir) {
      var scratchDir = CreateChocoScratchDir(buildDir);
      var installScriptPath = CreateChocoInstallScript(packageId, archiveUrl, scratchDir);
      var distPackage = CreateChocoPackage(packageId, packageVersion, username, scratchDir, installScriptPath);
      return Exec.Run("cpush", distPackage) == 0;
    }

    private static string CreateChocoPackage(string packageId, string packageVersion, string username, string scratchDir, string installScriptPath)
      => NuGetPackager.CreatePackage(
        scratchDir,
        Directory.GetCurrentDirectory(),
        packageId,
        packageVersion,
        new[] {new PackageFile(installScriptPath, "tools/chocolateyInstall.ps1"),},
        Enumerable.Empty<PackageDependency>(),
        new NuGetPackageMetadata(username,
                                 packageId,
                                 ImmutableDictionary<string, string>.Empty),
        "-NoPackageAnalysis");

    private static string CreateChocoScratchDir(string buildDir) {
      var chocoDistDir = Path.Combine(buildDir, "choco-dist-package");
      Directory.CreateDirectory(chocoDistDir);
      return chocoDistDir;
    }

    private static string CreateChocoInstallScript(string packageId, string archiveUrl, string scratchDir) {
      var chocoInstallScriptPath = Path.Combine(scratchDir, "chocolateyInstall.ps1");
      var chocolateyInstallScript = ChocolateyInstallScript(packageId, archiveUrl);
      File.WriteAllText(chocoInstallScriptPath, chocolateyInstallScript);
      return chocoInstallScriptPath;
    }

    private static string ChocolateyInstallScript(string packageId,
                                                  string archiveUrl)
      => $"Install-ChocolateyZipPackage '{packageId}' " +
         $"'{archiveUrl}' " +
         "\"$(Split-Path -parent $MyInvocation.MyCommand.Definition)\"";

    private static string BintrayArchiveDownloadUrl(string repositoryId,
                                                    string packageId,
                                                    string username,
                                                    string packageVersion)
      => $"https://dl.bintray.com/{username}/{repositoryId}/{packageId}-{packageVersion}.zip";

    private static string BintrayPublishPackageUrl(string packageId,
                                                   string repositoryId,
                                                   string username,
                                                   string packageVersion)
      => "https://api.bintray.com/" +
         $"content/{username}/{repositoryId}/{packageId}/" +
         $"{packageVersion}/{packageId}-{packageVersion}.zip?publish=1";

    private static string LoadBintrayCredentials(string username) {
      var apiKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                    "Bud",
                                    $"{username}@bintray.apikey");
      var apiKey = File.ReadAllText(apiKeyFile).Trim();
      return Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + apiKey));
    }
  }
}