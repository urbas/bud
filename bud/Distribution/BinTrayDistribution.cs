using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
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
                     .Map(archiveUrl => ChocoDistribution.PushToChoco(repositoryId, packadeId, packageVersion, archiveUrl, username, buildDir))
                     .GetOrElse(false));

    public static Option<string> PushToBintray(string package,
                                               string repositoryId,
                                               string packageId,
                                               string packageVersion,
                                               string username) {
      var packagePublishUrl = BintrayPublishPackageUrl(packageId, repositoryId, username, packageVersion);
      var apiKey = LoadBintrayApiKey(username);
      Console.WriteLine("Starting to upload to bintray...");
      using (var httpClient = new HttpClient()) {
        var credentials = ToBasicAuthCredentials(username, apiKey);
        httpClient.Timeout = TimeSpan.FromMinutes(15);
        Console.WriteLine($"Timeout: {httpClient.Timeout}");
        using (var content = new StreamContent(File.OpenRead(package))) {
          using (var request = new HttpRequestMessage(HttpMethod.Put, packagePublishUrl)) {
            request.Headers.Add("Authorization", $"Basic {credentials}");
            request.Content = content;
            request.Method = HttpMethod.Put;
            var responseTask = httpClient.SendAsync(request);
            responseTask.Wait(TimeSpan.FromMinutes(15));
            using (var response = responseTask.Result) {
              var uploadSuccess = response.StatusCode == HttpStatusCode.Created;
              Console.WriteLine($"Upload to bintray result code: {response.StatusCode}");
              var responseContentTask = response.Content.ReadAsStringAsync();
              responseContentTask.Wait();
              Console.WriteLine($"Upload to bintray response body: {responseContentTask.Result}");
              return uploadSuccess ?
                       BintrayArchiveDownloadUrl(repositoryId, packageId, username, packageVersion) :
                       Option.None<string>();
            }
          }
        }
      }
    }

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

    private static string LoadBintrayApiKey(string username) {
      var apiKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                    "Bud",
                                    $"{username}@bintray.apikey");
      return File.ReadAllText(apiKeyFile).Trim();
    }

    private static string ToBasicAuthCredentials(string username, string apiKey)
      => Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + apiKey));
  }
}