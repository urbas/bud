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
  private const string BudExeShimPath = "usr/bin/bud";

  public static void CreateUbuntuPackage(IConfig config, SemanticVersion version) {
    using (var scratchDir = new TemporaryDirectory()) {
      var outputDebPackage = Path.Combine(scratchDir.Path, GetUbuntuPackageFileName(version));
      var budLibFiles = PlaceBudLibFilesIntoScratchDir(config, scratchDir.Path);
      AddDescriptorToDebPackage(config, outputDebPackage, scratchDir.Path);
      AddControlToDebPackage(config, outputDebPackage, scratchDir.Path, version, budLibFiles);
      AddBudLibFilesToDebPackage(outputDebPackage, scratchDir.Path);
      UploadDebPackageToDropbox(version, outputDebPackage);
    }
  }

  private static ImmutableList<string> PlaceBudLibFilesIntoScratchDir(IConfig config, string scratchDir) {
    var budLibFiles = CopyBudLibsToScratchDir(scratchDir, Directory.EnumerateFiles(Build.GetBudDistDir(config)));
    PlaceBudShimToScratchDir(config, scratchDir);
    return budLibFiles.Add(BudExeShimPath);
  }

  private static void AddDescriptorToDebPackage(IConfig config, string outputDebPackage, string scratchDir) {
    var debianDescriptorFile = Path.Combine(scratchDir, "debian-binary");
    File.WriteAllText(debianDescriptorFile, "2.0\n");
    AddFileToArchive(outputDebPackage, debianDescriptorFile);
  }

  private static void AddControlToDebPackage(IConfig config, string outputDebPackage, string scratchDir, SemanticVersion version, IEnumerable<string> packagedBinaryFiles) {
    const string controlTarGz = "control.tar.gz";
    const string controlFileName = "control";
    const string md5SumsFileName = "md5sums";
    CreateControlFile(scratchDir, version, GetUbuntuPackageTemplateDir(config), controlFileName, (int) Math.Ceiling(CalculateSizeOfFilesInBytes(scratchDir, packagedBinaryFiles) / 1024));
    CreateMd5SumsFile(scratchDir, packagedBinaryFiles, md5SumsFileName);
    var controlArchive = CreateTarGz(scratchDir, controlTarGz, controlFileName, md5SumsFileName);
    AddFileToArchive(outputDebPackage, controlArchive);
  }

  private static void AddBudLibFilesToDebPackage(string outputDebPackage, string scratchDir) {
    var dataTarGz = CreateTarGz(scratchDir, "data.tar.gz", "usr");
    AddFileToArchive(outputDebPackage, dataTarGz);
  }

  private static void UploadDebPackageToDropbox(SemanticVersion version, string outputDebPackage) {
    var distUbuntuPackage = Path.Combine(DistributionZipPackaging.DropboxDistributionDir, GetUbuntuPackageFileName(version));
    File.Copy(outputDebPackage, distUbuntuPackage);
  }


  private static void PlaceBudShimToScratchDir(IConfig config, string scratchDir) {
    var absoluteBudShimExe = Path.Combine(scratchDir, BudExeShimPath);
    Directory.CreateDirectory(Path.GetDirectoryName(absoluteBudShimExe));
    File.WriteAllText(absoluteBudShimExe, "#! /bin/bash\n\n/usr/bin/mono /usr/lib/bud/bud.exe $@\n");
  }

  private static ImmutableList<string> CopyBudLibsToScratchDir(string scratchDir, IEnumerable<string> filesToPackage) {
    var scratchBudLibsDir = Path.Combine(scratchDir, "usr", "lib", "bud");
    Directory.CreateDirectory(scratchBudLibsDir);
    return filesToPackage.Select(srcFile => {
      var fileName = Path.GetFileName(srcFile);
      File.Copy(srcFile, Path.Combine(scratchBudLibsDir, fileName));
      return "usr/lib/bud/" + fileName;
    }).ToImmutableList();
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

  private static string GetUbuntuPackageTemplateDir(IConfig config) {
    return Path.Combine(config.GetDeploymentTemplatesDir(), "UbuntuPackage");
  }

  private static string GetUbuntuPackageFileName(SemanticVersion version) {
    return string.Format("bud_{0}_i386.deb", version);
  }
}