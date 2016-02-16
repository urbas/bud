using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static System.IO.Directory;
using static System.IO.Path;

namespace Bud.IO {
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
      Delete(Path, true);
    }

    public List<string> FindOpenedFiles()
      => EnumerateFiles(Path, "*", SearchOption.AllDirectories).Where(IsFileLocked)
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

    public string CreateFilePath(params string[] targetPath)
      => targetPath.Length == 0 ?
           Combine(Path, Guid.NewGuid().ToString()) :
           Combine(new[] {Path}.Concat(targetPath).ToArray());

    private string CreateFilePathAndDir(string[] targetPath) {
      var file = CreateFilePath(targetPath);
      CreateDirectory(GetDirectoryName(file));
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
      var newDir = Combine(new[] {Path}.Concat(subDirPath).ToArray());
      CreateDirectory(newDir);
      return newDir;
    }

    private static string CreateDirectoryInTempDir() {
      string baseDir = GetTempPath();
      string tempDir;
      do {
        tempDir = Combine(baseDir, Guid.NewGuid().ToString());
      } while (Exists(tempDir));
      CreateDirectory(tempDir);
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