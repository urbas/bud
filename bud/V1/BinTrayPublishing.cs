using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using Bud.Util;
using static Bud.V1.Basic;

namespace Bud.V1 {
  public static class BinTrayPublishing {
    /// <summary>
    ///   The file to be pushed to a BinTray generic repository.
    /// </summary>
    public static readonly Key<IObservable<string>> PackageFile = nameof(PackageFile);

    /// <summary>
    ///   Pushes the <see cref="PackageFile" /> to a generic BinTray repository.
    ///   It produces an observable stream of URLs from which the pushed <see cref="PackageFile"/>
    ///   can be downloaded (via HTTP get).
    /// </summary>
    public static readonly Key<IObservable<string>> Push = nameof(Push);

    /// <summary>
    ///   The ID of the generic BinTray repository to which you want to push the <see cref="PackageFile" />.
    /// </summary>
    public static readonly Key<string> RepositoryId = nameof(RepositoryId);

    /// <summary>
    ///   The ID of the package to be pushed to the generic BinTray repository.
    /// </summary>
    public static readonly Key<string> PackageId = nameof(PackageId);

    /// <summary>
    ///   The version of the package to be pushed to the generic BinTray repository.
    /// </summary>
    public static readonly Key<string> PackageVersion = nameof(PackageVersion);

    /// <summary>
    ///   The username to use when uploading to the generic BinTray repository.
    /// </summary>
    public static readonly Key<string> Username = nameof(Username);

    /// <summary>
    ///   This project pushes the <see cref="PackageFile" /> to a generic BinTray repository.
    ///   <see cref="PackageFile" /> will be pushed evry time it changes.
    /// </summary>
    /// <param name="projectId">the project's identifier.</param>
    /// <param name="packageFile">see <see cref="PackageFile" />.</param>
    /// <param name="repositoryId">see <see cref="RepositoryId" />.</param>
    /// <param name="packageId">see <see cref="PackageId" />.</param>
    /// <param name="packageVersion">see <see cref="PackageVersion" />.</param>
    /// <param name="username">see <see cref="Username" />.</param>
    /// <param name="baseDir">the base directory in which this project will place all its build artifacts.</param>
    /// <returns>the configured project.</returns>
    public static Conf Project(string projectId,
                               Func<IConf, IObservable<string>> packageFile,
                               Func<IConf, string> repositoryId,
                               Func<IConf, string> packageId,
                               Func<IConf, string> packageVersion,
                               Func<IConf, string> username,
                               Option<string> baseDir = default(Option<string>))
      => BareProject(projectId, baseDir)
        .Init(PackageFile, packageFile)
        .Init(RepositoryId, repositoryId)
        .Init(PackageId, packageId)
        .Init(PackageVersion, packageVersion)
        .Init(Username, username)
        .Init(Push, PushToGenericRepo);

    private static IObservable<string> PushToGenericRepo(IConf c) {
      var repositoryId = RepositoryId[c];
      string packageId = PackageId[c];
      string packageVersion = PackageVersion[c];
      string username = Username[c];
      return PackageFile[c].Select(file => PushToBintray(file,
                                                         repositoryId,
                                                         packageId,
                                                         packageVersion,
                                                         username));
    }

    public static string PushToBintray(string package,
                                       string repositoryId,
                                       string packageId,
                                       string packageVersion,
                                       string username)
      => PushToBintray(() => File.OpenRead(package),
                       repositoryId,
                       packageId,
                       packageVersion,
                       username,
                       Path.GetExtension(package));

    public static string PushToBintray(Func<Stream> contentFetcher,
                                       string repositoryId,
                                       string packageId,
                                       string packageVersion,
                                       string username,
                                       string fileExtension) {
      var packagePublishUrl = BintrayPublishPackageUrl(repositoryId, packageId, username, packageVersion, fileExtension);
      var apiKey = LoadBintrayApiKey(username);
      Console.WriteLine("Starting to upload to bintray...");
      using (var httpClient = new HttpClient()) {
        var credentials = ToBasicAuthCredentials(username, apiKey);
        httpClient.Timeout = TimeSpan.FromMinutes(15);
        Console.WriteLine($"Timeout: {httpClient.Timeout}");
        using (var content = new StreamContent(contentFetcher())) {
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
              if (!uploadSuccess) {
                throw new Exception($"Could not upload the package '{packageId}' to '{packagePublishUrl}'.");
              }
              return BintrayArchiveDownloadUrl(repositoryId, packageId, username, packageVersion, fileExtension);
            }
          }
        }
      }
    }

    public static string BintrayArchiveDownloadUrl(string repositoryId,
                                                   string packageId,
                                                   string username,
                                                   string packageVersion,
                                                   string fileExtension)
      => "https://dl.bintray.com/" +
         $"{username}/{repositoryId}/" + PackageFileName(packageId, packageVersion, fileExtension);

    public static string BintrayPublishPackageUrl(string repositoryId,
                                                  string packageId,
                                                  string username,
                                                  string packageVersion,
                                                  string fileExtension)
      => "https://api.bintray.com/" +
         $"content/{username}/{repositoryId}/{packageId}/" +
         $"{packageVersion}/{PackageFileName(packageId, packageVersion, fileExtension)}?publish=1";

    private static string PackageFileName(string packageId,
                                          string packageVersion,
                                          string fileExtension)
      => $"{packageId}-{packageVersion}{fileExtension}";

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