using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Bud.Resources {
  public static class EmbeddedResources {
    public static async Task CopyTo(Assembly assembly, string resourceName, string targetPath) {
      Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
      using (var sourceStream = assembly.GetManifestResourceStream(resourceName)) {
        using (var destinationStream = new FileStream(targetPath, FileMode.CreateNew)) {
          await sourceStream.CopyToAsync(destinationStream);
        }
      }
    }
  }
}