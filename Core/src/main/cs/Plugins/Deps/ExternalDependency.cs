using NuGet;

namespace Bud.Plugins.Deps {
  public class ExternalDependency {
    public readonly string Id;
    public readonly SemanticVersion Version;

    public ExternalDependency(string id, string version) : this(id, version == null ? null : SemanticVersion.Parse(version)) {}

    public ExternalDependency(string id, SemanticVersion version) {
      this.Version = version;
      this.Id = id;
    }

    public override string ToString() {
      if (Version != null) {
        return string.Format("{0}@{1}", Id, Version);
      }
      return Id;
    }
  }
}