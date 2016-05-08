using System;
using System.Reactive.Linq;
using Bud.Util;
using static Bud.Dist.BinTrayDistribution;
using static Bud.V1.Basic;

namespace Bud.V1 {
  public static class BinTrayPublishing {
    /// <summary>
    ///   The file to be pushed to a BinTray generic repository.
    /// </summary>
    public static readonly Key<IObservable<string>> PackageFile = nameof(PackageFile);

    /// <summary>
    ///   Pushes the <see cref="PackageFile" /> to a generic BinTray repository.
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
        .Init(PackageVersion, PackageVersion)
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
  }
}