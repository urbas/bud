using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bud.TempDir {
  public class TemporaryDirectory : IDisposable {
    public TemporaryDirectory() {
      Path = CreateDirectoryInTempDir();
    }

    public string Path { get; }

    public void Dispose() {
      var openedFiles = FindOpenedFiles();
      if (openedFiles.Count > 0) {
        throw new Exception($"Could not delete the temporary directory {Path}. There were some locked files: {string.Join(", ", openedFiles)}");
      }
      Directory.Delete(Path, true);
    }

    public List<string> FindOpenedFiles()
      => Directory.EnumerateFiles(Path, "*", SearchOption.AllDirectories).Where(IsFileLocked)
                                                               .ToList();

    public override string ToString() => Path;

    public string CreateEmptyFile(params string[] targetPath)
      => CreateFile(string.Empty, targetPath);

    public string CreateFile(string content, params string[] targetPath) {
      var file = CreateFilePathAndDir(targetPath);
      File.WriteAllText(file, content);
      return file;
    }

    public string CreateFileFromResource(string resourceName, params string[] targetPath) {
      using (var packagesConfigStream = Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName)) {
        return CreateFile(packagesConfigStream, targetPath);
      }
    }

    public string CreatePath(params string[] targetPath)
      => targetPath.Length == 0 ?
           System.IO.Path.Combine(Path, Guid.NewGuid().ToString()) :
           System.IO.Path.Combine(new[] {Path}.Concat(targetPath).ToArray());

    private string CreateFilePathAndDir(string[] targetPath) {
      var file = CreatePath(targetPath);
      Directory.CreateDirectory(System.IO.Path.GetDirectoryName(file));
      return file;
    }

    public string CreateFile(Stream content, params string[] targetPath) {
      var file = CreateFilePathAndDir(targetPath);
      using (var fileStream = File.Open(file, FileMode.Create, FileAccess.Write)) {
        content.CopyTo(fileStream);
      }
      return file;
    }

    public string CreateDir(params string[] subDirPath) {
      var newDir = System.IO.Path.Combine(new[] {Path}.Concat(subDirPath).ToArray());
      Directory.CreateDirectory(newDir);
      return newDir;
    }

    private static string CreateDirectoryInTempDir() {
      string baseDir = System.IO.Path.GetTempPath();
      string tempDir;
      do {
        tempDir = System.IO.Path.Combine(baseDir, Guid.NewGuid().ToString());
      } while (Directory.Exists(tempDir));
      Directory.CreateDirectory(tempDir);
      return tempDir;
    }

    private static bool IsFileLocked(string file) {
      FileStream stream = null;
      try {
        stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
      } catch (IOException) {
        return true;
      } finally {
        stream?.Close();
      }
      return false;
    }
  }
}