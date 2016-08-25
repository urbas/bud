using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using Bud.NuGet;
using static Bud.V1.Basic;

namespace Bud.V1 {
  public static class ChocoPublishing {
    /// <summary>
    ///   The ID of the package to be pushed to Chocolatey.
    /// </summary>
    public static readonly Key<string> PackageId = nameof(PackageId);

    /// <summary>
    ///   The version of the package to be pushed to the generic BinTray repository.
    /// </summary>
    public static readonly Key<string> PackageVersion = nameof(PackageVersion);

    /// <summary>
    ///   This is a stream of URLs from which a ZIP archive can be downloaded (via HTTP GET).
    ///   This file must contain files required for installation of the package (as defined
    ///   by Chocolatey).
    /// </summary>
    public static readonly Key<IObservable<string>> ArchiveUrl = nameof(ArchiveUrl);

    /// <summary>
    ///   Extra metadata to be added to the pushed package.
    /// </summary>
    public static readonly Key<NuGetPackageMetadata> PackageMetadata = nameof(PackageMetadata);

    /// <summary>
    ///   Pushes the <see cref="PackageId" /> to Chocolatey. It produces an observable stream
    ///   of URLs from which the pushed packages can be downoaded.
    /// </summary>
    public static readonly Key<IObservable<string>> Push = nameof(Push);

    /// <summary>
    ///   This project pushes a package to Chocolatey.
    /// </summary>
    /// <param name="projectId">the project's identifier.</param>
    /// <param name="packageId">see <see cref="PackageId" />.</param>
    /// <param name="packageVersion">see <see cref="PackageVersion" />.</param>
    /// <param name="packageMetadata">see <see cref="PackageMetadata" />.</param>
    /// <param name="archiveUrl">see <see cref="ArchiveUrl" />.</param>
    /// <param name="baseDir">the base directory in which this project will place all its build artifacts.</param>
    /// <returns>the configured project.</returns>
    public static Conf Project(string projectId,
                               Func<IConf, string> packageId,
                               Func<IConf, string> packageVersion,
                               Func<IConf, NuGetPackageMetadata> packageMetadata,
                               Func<IConf, IObservable<string>> archiveUrl,
                               Option<string> baseDir = default(Option<string>))
      => BareProject(projectId, baseDir)
        .Init(PackageId, packageId)
        .Init(PackageVersion, packageVersion)
        .Init(PackageMetadata, packageMetadata)
        .Init(ArchiveUrl, archiveUrl)
        .Init(Push, PushToChocolatey);

    private static IObservable<string> PushToChocolatey(IConf c)
      => ArchiveUrl[c].Select(archiveUrl => PushToChoco(PackageId[c],
                                                        PackageVersion[c],
                                                        archiveUrl,
                                                        BuildDir[c],
                                                        PackageMetadata[c]));

    public static string PushToChoco(string packageId,
                                     string packageVersion,
                                     string archiveUrl,
                                     string buildDir,
                                     NuGetPackageMetadata packageMetadata) {
      var scratchDir = CreateChocoScratchDir(buildDir);
      var installScriptPath = CreateChocoInstallScript(packageId, archiveUrl, scratchDir);
      var distPackage = CreateChocoPackage(packageId, packageVersion, scratchDir, installScriptPath, packageMetadata);
      Console.WriteLine($"Starting to push to chocolatey ...");
      // TODO: use nuget instead. Read the ApiKey from somewhere.
      //      var success = NuGetExecutable.Instance.Run($"push {distPackage} -source https://chocolatey.org/ -NonInteractive");
      var success = Exec.Run("cpush", distPackage).ExitCode == 0;
      Console.WriteLine($"Push to chocolatey success: {success}");
      // TODO: Return the actual URL
      return "FOO BAR!";
    }

    private static string CreateChocoPackage(string packageId, string packageVersion, string scratchDir, string installScriptPath, NuGetPackageMetadata packageMetadata) {
      return NuGetPackager.CreatePackage(
        scratchDir,
        Directory.GetCurrentDirectory(),
        packageId,
        packageVersion,
        new[] {new PackageFile(installScriptPath, "tools/chocolateyInstall.ps1"),},
        Enumerable.Empty<PackageDependency>(),
        packageMetadata,
        "-NoPackageAnalysis");
    }

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
  }
}