using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using Bud.V1;
using static Bud.V1.Api;

public class BudBuild : IBuild {
  public Conf Init()
    => Projects(CsApp("Bud")
                  .SetValue(ProjectVersion, "0.5.0-pre-2")
                  .Set("/push-to-bintray", PushToBintray),
                CsLibrary("Bud.Test")
                  .Add(Dependencies, "../Bud"));

  private static IObservable<bool> PushToBintray(IConf c)
    => DistributionArchive[c]
      .Select(archive => PushToBintray("bud", "matej", ProjectVersion[c], archive));

  private static bool PushToBintray(string packadeId, string username, string packageVersion, string archive) {
    var packageResourceAddress = BintrayPublishPackageUrl(packadeId, packadeId, username, packageVersion);
    var credentials = LoadBintrayCredentials(username);
    using (var httpClient = new HttpClient()) {
      using (var content = new StreamContent(File.OpenRead(archive))) {
        content.Headers.Remove("Content-Type");
        content.Headers.Add("Content-Type", "application/octet-stream");

        using (var request = new HttpRequestMessage(HttpMethod.Put, packageResourceAddress)) {
          request.Headers.Add("Authorization", $"Basic {credentials}");
          request.Headers.Remove("Expect");
          request.Headers.Add("Expect", "");
          request.Content = content;
          request.Method = HttpMethod.Put;

          var responseTask = httpClient.SendAsync(request);
          responseTask.Wait();
          using (var response = responseTask.Result) {
            Console.WriteLine($"StatusCode: {response.StatusCode}");
            return response.StatusCode == HttpStatusCode.Created;
          }
        }
      }
    }
  }

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