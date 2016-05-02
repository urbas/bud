using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Convert;
using static System.Environment.SpecialFolder;
using static System.Text.Encoding;

namespace Bud.Dist {
  public static class BinTray {
    /// <summary>
    ///   The <paramref name="contentFetcher" /> will be called and the stream that
    ///   it returns will be closed after the contents of this stream are uploaded.
    /// </summary>
    /// <returns>the URL from which you can fetch the uploaded package.</returns>
    public static string PushToGenericRepo(Func<Stream> contentFetcher,
                                           string repositoryId,
                                           string packageId,
                                           string packageVersion,
                                           string username,
                                           string fileExtension,
                                           TimeSpan uploadTimeout) {
      var packagePublishUrl = BintrayPublishPackageUrl(packageId, repositoryId, username, packageVersion, fileExtension);
      var apiKey = LoadBintrayApiKey(username);
      using (var httpClient = new HttpClient()) {
        var credentials = ToBasicAuthCredentials(username, apiKey);
        httpClient.Timeout = uploadTimeout;
        using (var content = new StreamContent(contentFetcher())) {
          return SendHttpPutRequest(repositoryId, packageId, packageVersion, username, fileExtension, packagePublishUrl, credentials, content, httpClient);
        }
      }
    }

    /// <summary>
    ///   Uploads the file <paramref name="package" /> to BinTray.
    /// </summary>
    /// <returns>the URL from which you can fetch the uploaded package.</returns>
    public static string PushToGenericRepo(string package,
                                           string repositoryId,
                                           string packageId,
                                           string packageVersion,
                                           string username,
                                           string fileExtension,
                                           TimeSpan uploadTimeout)
      => PushToGenericRepo(() => File.OpenRead(package),
                           repositoryId,
                           packageId,
                           packageVersion,
                           username,
                           fileExtension,
                           uploadTimeout);

    private static string SendHttpPutRequest(string repositoryId,
                                             string packageId,
                                             string packageVersion,
                                             string username,
                                             string fileExtension,
                                             string packagePublishUrl,
                                             string credentials,
                                             HttpContent content,
                                             HttpClient httpClient) {
      using (var request = new HttpRequestMessage(HttpMethod.Put, packagePublishUrl)) {
        request.Headers.Add("Authorization", $"Basic {credentials}");
        request.Content = content;
        request.Method = HttpMethod.Put;
        var responseTask = httpClient.SendAsync(request);
        responseTask.Wait(TimeSpan.FromMinutes(15));
        return ProcessUploadResult(repositoryId, packageId, packageVersion, username, fileExtension, responseTask);
      }
    }

    private static string ProcessUploadResult(string repositoryId,
                                              string packageId,
                                              string packageVersion,
                                              string username,
                                              string fileExtension,
                                              Task<HttpResponseMessage> responseTask) {
      using (var response = responseTask.Result) {
        var uploadSuccess = response.StatusCode == HttpStatusCode.Created;
        if (!uploadSuccess) {
          var responseContentTask = response.Content.ReadAsStringAsync();
          responseContentTask.Wait();
          throw new Exception($"Upload of package {packageId} (version {packageVersion}) to BinTray repository {repositoryId} failed. Response from BinTray: {responseContentTask.Result}");
        }
        return BintrayArchiveDownloadUrl(repositoryId, packageId, username, packageVersion, fileExtension);
      }
    }

    private static string BintrayArchiveDownloadUrl(string repositoryId,
                                                    string packageId,
                                                    string username,
                                                    string packageVersion,
                                                    string fileExtension)
      => "https://dl.bintray.com/" +
         $"{username}/{repositoryId}/" +
         $"{packageId}-{packageVersion}.{fileExtension}";

    private static string BintrayPublishPackageUrl(string packageId,
                                                   string repositoryId,
                                                   string username,
                                                   string packageVersion,
                                                   string fileExtension)
      => "https://api.bintray.com/" +
         $"content/{username}/{repositoryId}/{packageId}/" +
         $"{packageVersion}/{packageId}-{packageVersion}.{fileExtension}?publish=1";

    private static string LoadBintrayApiKey(string username) {
      var appDataDir = Environment.GetFolderPath(LocalApplicationData);
      var apiKeyFile = Path.Combine(appDataDir,
                                    "Bud",
                                    $"{username}@bintray.apikey");
      return File.ReadAllText(apiKeyFile).Trim();
    }

    private static string ToBasicAuthCredentials(string username, string apiKey)
      => ToBase64String(ASCII.GetBytes(username + ":" + apiKey));
  }
}