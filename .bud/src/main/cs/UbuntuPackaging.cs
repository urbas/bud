using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Antlr4.StringTemplate;
using Bud;
using Bud.Cli;
using Bud.IO;
using Bud.Util;
using NuGet;

internal static class UbuntuPackaging {
  public static void CreateUbuntuPackage(IConfig config, SemanticVersion version) {
    using (var scratchDir = new TemporaryDirectory()) {
      var ubuntuPackageDir = GetUbuntuPackageDir(config);
      var debPackage = Path.Combine(scratchDir.Path, GetUbuntuPackageFileName(version));
      const string relativeBudShimExe = "usr/bin/bud";
      var relativeFilesToPackage = CopyBudBinariesToUsrDir(scratchDir.Path, Directory.EnumerateFiles(Build.GetBudDistDir(config))).Add(relativeBudShimExe);
      CopyBudShimExe(scratchDir, relativeBudShimExe, ubuntuPackageDir);
      AddFileToArchive(debPackage, Path.Combine(ubuntuPackageDir, "debian-binary"));
      AddControlDataToDebPackage(debPackage, scratchDir.Path, ubuntuPackageDir, version, relativeFilesToPackage);
      AddDataToDebPackage(debPackage, scratchDir.Path);
      var distUbuntuPackage = Path.Combine(DistributionZipPackaging.DropboxDistributionDir, GetUbuntuPackageFileName(version));
      File.Copy(debPackage, distUbuntuPackage);
    }
  }


  private static void AddDataToDebPackage(string debPackage, string baseDir) {
    var dataTarXf = CreateTarGz(baseDir, "data.tar.gz", "usr");
    AddFileToArchive(debPackage, dataTarXf);
  }

  private static void CopyBudShimExe(TemporaryDirectory tmpDir, string relativeBudShimExe, string ubuntuPackageDir) {
    var absoluteBudShimExe = Path.Combine(tmpDir.Path, relativeBudShimExe);
    Directory.CreateDirectory(Path.GetDirectoryName(absoluteBudShimExe));
    File.Copy(Path.Combine(ubuntuPackageDir, "bud"), absoluteBudShimExe);
  }

  private static ImmutableList<string> CopyBudBinariesToUsrDir(string destDir, IEnumerable<string> filesToPackage) {
    var destinationDir = Path.Combine(destDir, "usr", "lib", "bud");
    Directory.CreateDirectory(destinationDir);
    return filesToPackage.Select(file => {
      var fileName = Path.GetFileName(file);
      File.Copy(file, Path.Combine(destinationDir, fileName));
      return "usr/lib/bud/" + fileName;
    }).ToImmutableList();
  }

  private static void AddControlDataToDebPackage(string debPackage, string scratchDirectoryPath, string templatesDir, SemanticVersion version, IEnumerable<string> packagedBinaryFiles) {
    const string controlTarGz = "control.tar.gz";
    const string controlFileName = "control";
    const string md5SumsFileName = "md5sums";
    CreateControlFile(scratchDirectoryPath, version, templatesDir, controlFileName, (int) Math.Ceiling((double) CalculateSizeOfFilesInBytes(scratchDirectoryPath, packagedBinaryFiles) / 1024));
    CreateMd5SumsFile(scratchDirectoryPath, packagedBinaryFiles, md5SumsFileName);
    var controlArchive = CreateTarGz(scratchDirectoryPath, controlTarGz, controlFileName, md5SumsFileName);
    AddFileToArchive(debPackage, controlArchive);
  }

  private static double CalculateSizeOfFilesInBytes(string baseDir, IEnumerable<string> relativePaths) {
    return relativePaths.Select(file => Path.Combine(baseDir, file))
                        .Select(absolutePath => new FileInfo(absolutePath).Length)
                        .Sum();
  }

  private static void CreateMd5SumsFile(string baseDir, IEnumerable<string> relativeFilePaths, string md5SumsFileName) {
    using (new TemporaryDirChange(baseDir)) {
      using (var md5SumsFileWriter = File.CreateText(Path.Combine(baseDir, md5SumsFileName))) {
        var md5Algo = MD5.Create();
        foreach (var file in relativeFilePaths) {
          using (var fileStream = File.OpenRead(file)) {
            StringUtils.ToHexString(md5Algo.ComputeHash(fileStream), md5SumsFileWriter);
            md5SumsFileWriter.Write("  ");
            md5SumsFileWriter.Write(file);
            md5SumsFileWriter.Write('\n');
          }
        }
      }
    }
  }

  private static void CreateControlFile(string scratchDirectoryPath, SemanticVersion version, string templatesDir, string controlFileName, int sizeOfPackageKb) {
    var templateContent = File.ReadAllText(Path.Combine(templatesDir, controlFileName));
    var template = new Template(templateContent, '%', '%')
      .Add("version", version)
      .Add("installedSize", sizeOfPackageKb);
    var outputControlFile = Path.Combine(scratchDirectoryPath, controlFileName);
    using (var outputControlFileWriter = File.CreateText(outputControlFile)) {
      template.Write(new AutoIndentWriter(outputControlFileWriter));
    }
  }

  private static string CreateTarGz(string baseDir, string tarballName, params string[] filesToAddToArchive) {
    return CreateTar(baseDir, tarballName, 'z', filesToAddToArchive, "--numeric-owner", "--owner=0", "--group=0");
  }

  private static string CreateTar(string baseDir, string tarballName, char archiveType, IEnumerable<string> filesToAddToArchive, params string[] additionalArguments) {
    using (new TemporaryDirChange(baseDir)) {
      var tarCommandLineArgs = new List<string> {"-C", baseDir, "-c", "-" + archiveType, "-f", tarballName};
      tarCommandLineArgs.AddRange(additionalArguments);
      tarCommandLineArgs.AddRange(filesToAddToArchive.Select(file => "./" + file));
      ProcessBuilder.Execute("tar", tarCommandLineArgs);
    }
    return Path.Combine(baseDir, tarballName);
  }

  private static void AddFileToArchive(string archivePath, string fileToAdd) {
    ProcessBuilder.Execute("ar", "r", archivePath, fileToAdd);
  }

  private static string GetUbuntuPackageDir(IConfig config) {
    return Path.Combine(config.GetDeploymentTemplatesDir(), "UbuntuPackage");
  }

  private static string GetUbuntuPackageFileName(SemanticVersion version) {
    return string.Format("bud_{0}_i386.deb", version);
  }
}