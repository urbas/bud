using System.Threading.Tasks;
using NuGet;

namespace Bud.Plugins.Dependencies {
  public class ExternalDependency	{
    public readonly string Id;
    public readonly SemanticVersion Version;

    public ExternalDependency(string id, string version) : this(id, version == null ? null : SemanticVersion.Parse(version)) {}

    public ExternalDependency(string id, SemanticVersion version) {
      this.Version = version;
      this.Id = id;
    }
	}
}
