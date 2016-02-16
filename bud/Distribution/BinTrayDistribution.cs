using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using Bud.V1;
using static Bud.V1.Api;

namespace Bud.Distribution {
  public class BinTrayDistribution {
    public static IObservable<bool> Distribute(IConf c) {
      var projectId = ProjectId[c];
      return Distribute(c, projectId, projectId, Environment.UserName);
    }

    public static IObservable<bool> Distribute(IConf c, string repositoryId, string packadeId, string username)
      => DistributionArchive[c]
        .Select(archive => PushToBintray(repositoryId, packadeId, username, archive, ProjectVersion[c])
                           && PushToChocolatey());

    private static bool PushToBintray(string repositoryId, string packadeId, string username, string package, string packageVersion) {
      var packagePublishUrl = BintrayPublishPackageUrl(packadeId, repositoryId, username, packageVersion);
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
              Console.WriteLine($"[PushToBintray] StatusCode: {response.StatusCode}");
              return response.StatusCode == HttpStatusCode.Created;
            }
          }
        }
      }
    }

    private static bool PushToChocolatey() {
      return false;
    }

    private static string ChocolateyInstallScript(string repositoryId,
                                                  string packageId,
                                                  string username,
                                                  string packageVersion)
      => $"Install-ChocolateyZipPackage '{packageId}' " +
         $"'https://dl.bintray.com/{username}/{repositoryId}/{packageId}-{packageVersion}.zip' " +
         " \"$(Split-Path -parent $MyInvocation.MyCommand.Definition)\"";

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