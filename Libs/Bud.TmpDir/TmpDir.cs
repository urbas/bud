using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static System.IO.Path;

namespace Bud {
  /// <summary>
  /// Creates a temporary directory and deletes it once this oject is disposed.
  /// </summary>
  public class TmpDir : IDisposable {

    /// <summary>
    /// Creates a temporary directory in the user's temporary directory.
    /// </summary>
    public TmpDir() {
      Path = CreateDirectoryInTempDir();
    }

    /// <summary>
    /// The absolute path to the temporary directory.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Deletes the temporary directory created by this instance.
    /// </summary>
    /// <remarks>
    ///   If there was an exception during the deletion and this method finds some locked
    ///   files, then this method will throw a wrapping exception that lists the locked files.
    /// </remarks>
    public void Dispose() {
      try {
        Directory.Delete(Path, true);
      } catch (Exception e) {
        var openedFiles = FindOpenedFiles();
        if (openedFiles.Count > 0) {
          throw new Exception($"Could not delete the temporary directory {Path}. " +
                              $"There were some locked files: {string.Join(", ", openedFiles)}",
                              e);
        }
        throw;
      }
    }

    /// <param name="targetPath">
    ///   a list of path components. These are combined to produce the path of the file to create.
    /// </param>
    /// <returns>the path of the created file</returns>
    public string CreateEmptyFile(params string[] targetPath)
      => CreateFile(string.Empty, targetPath);

    /// <param name="content">the content of the file to create.</param>
    /// <param name="targetPath">
    ///   a list of path components. These are combined to produce the path of the file to create.
    /// </param>
    /// <returns>the path of the created file</returns>
    public string CreateFile(string content, params string[] targetPath) {
      var file = CreateFilePathAndDir(targetPath);
      File.WriteAllText(file, content);
      return file;
    }

    /// <param name="subDirPath">
    ///   a list of path components. These are combined to produce the path of the directory to create.
    /// </param>
    /// <returns>the path of the created directory.</returns>
    public string CreateDir(params string[] subDirPath) {
      var newDir = Combine(new[] {Path}.Concat(subDirPath).ToArray());
      Directory.CreateDirectory(newDir);
      return newDir;
    }

    /// <summary>
    ///   creates a file with the contents of the given embedded resource.
    /// </summary>
    /// <param name="resourceName">the name of the embedded resource file.</param>
    /// <param name="targetPath">
    ///   a list of path components. These are combined to produce the path of the file to create.
    /// </param>
    /// <returns>the path of the created file</returns>
    public string CreateFileFromResource(string resourceName, params string[] targetPath) {
      using (var packagesConfigStream = Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName)) {
        return CreateFile(packagesConfigStream, targetPath);
      }
    }

    /// <summary>
    ///   Returns a path within this tempororary directory without creating a file.
    /// </summary>
    /// <param name="targetPath">
    ///   a list of path components. These are combined to produce the path within the temporary directory.
    ///   If this list is empty, then this method will create a new direct sub-path whose name will be a guid.
    /// </param>
    /// <returns>the created path.</returns>
    public string CreatePath(params string[] targetPath)
      => targetPath.Length == 0 ?
           Combine(Path, Guid.NewGuid().ToString()) :
           Combine(new[] {Path}.Concat(targetPath).ToArray());

    /// <param name="content">the content the new file should contain.</param>
    /// <param name="targetPath">
    ///   a list of path components. These are combined to produce the path of the file to create.
    /// </param>
    /// <returns>the path of the created file</returns>
    public string CreateFile(Stream content, params string[] targetPath) {
      var file = CreateFilePathAndDir(targetPath);
      using (var fileStream = File.Open(file, FileMode.Create, FileAccess.Write)) {
        content.CopyTo(fileStream);
      }
      return file;
    }

    /// <returns>a list of all open files in this temporary directory.</returns>
    public List<string> FindOpenedFiles()
      => Directory.EnumerateFiles(Path, "*", SearchOption.AllDirectories)
                  .Where(IsFileLocked)
                  .ToList();

    /// <returns>the path of this temporary directory.</returns>
    public override string ToString() => Path;

    /// <summary>
    ///   Checks whether the given file can be opened for reading and writing
    ///   with <see cref="FileShare.None"/> mode.
    /// </summary>
    /// <param name="file">
    ///   the file to check whether it is locked (opened by another process).
    /// </param>
    /// <returns>returns <c>true</c> if and only if the file can be opened in
    /// non-sharing mode. This means that no other process is locking the file.</returns>
    public static bool IsFileLocked(string file) {
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

    private string CreateFilePathAndDir(string[] targetPath) {
      var file = CreatePath(targetPath);
      Directory.CreateDirectory(GetDirectoryName(file));
      return file;
    }

    private static string CreateDirectoryInTempDir() {
      string baseDir = GetTempPath();
      string tempDir;
      do {
        tempDir = Combine(baseDir, Guid.NewGuid().ToString());
      } while (Directory.Exists(tempDir));
      Directory.CreateDirectory(tempDir);
      return tempDir;
    }
  }
}