using System.Collections.Generic;
using System.IO;
using static System.IO.Directory;
using static System.IO.File;
using static System.IO.Path;
using static Bud.NuGet.NuGetExecutable;

namespace Bud.NuGet {
  public class NuGetPackager : IPackager {
    public string Pack(string outputDir, string packageId, string version, IEnumerable<PackageFile> files, IEnumerable<PackageDependency> packageDependencies, NuGetPackageMetadata packageMetadata) {
      var filesDir = Combine(outputDir, "files");
      var nupkgDir = Combine(outputDir, "nupkg");
      var nuspecFile = Combine(outputDir, $"{packageId}.nuspec");
      var nupkgFile = Combine(nupkgDir, $"{packageId}.{version}.nupkg");
      CreateDirectory(filesDir);
      CreateDirectory(nupkgDir);
      CopyPackageFiles(files, filesDir);
      CreateNuspecFile(nuspecFile, packageId, version, packageDependencies, packageMetadata);
      ExecuteNuGet($"pack {nuspecFile} " +
                   $"-OutputDirectory {nupkgDir} " +
                   $"-BasePath {filesDir}");
      return nupkgFile;
    }

    private static void CopyPackageFiles(IEnumerable<PackageFile> files, string filesDir) {
      Delete(filesDir, true);
      foreach (var packageFile in files) {
        var destFileName = Combine(filesDir, packageFile.PathInPackage);
        CreateDirectory(GetDirectoryName(destFileName));
        Copy(packageFile.FileToPackage, destFileName, true);
      }
    }

    private static void CreateNuspecFile(string nuspecFile, string packageId, string version, IEnumerable<PackageDependency> packageDependencies, NuGetPackageMetadata packageMetadata) {
      using (var stream = Open(nuspecFile, FileMode.Create, FileAccess.Write)) {
        using (var writer = new StreamWriter(stream)) {
          WriteNuspecContent(writer, packageId, version, packageDependencies, packageMetadata);
        }
      }
    }

    private static void WriteNuspecContent(TextWriter writer, string packageId, string version, IEnumerable<PackageDependency> packageDependencies, NuGetPackageMetadata packageMetadata) {
      writer.Write("<?xml version=\"1.0\"?>");
      writer.Write("<package xmlns=\"http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd\">");
      writer.Write("<metadata>");
      writer.Write($"<id>{packageId}</id>");
      writer.Write($"<version>{version}</version>");
      writer.Write($"<authors>{packageMetadata.Authors}</authors>");
      writer.Write($"<description>{packageMetadata.Description}</description>");
      foreach (var optionalField in packageMetadata.OptionalFields) {
        writer.Write($"<{optionalField.Key}>{optionalField.Value}</{optionalField.Key}>");
      }
      writer.Write("<dependencies>");
      foreach (var packageDependency in packageDependencies) {
        writer.Write($"<dependency id=\"{packageDependency.PackageId}\" version=\"{packageDependency.Version}\" />");
      }
      writer.Write("</dependencies>");
      writer.Write("</metadata>");
      writer.Write("</package>");
    }
  }
}