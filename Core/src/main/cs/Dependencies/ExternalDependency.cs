using NuGet;

namespace Bud.Dependencies {
  public class ExternalDependency {
    public readonly string Id;
    public readonly IVersionSpec Version;

    public ExternalDependency(string id, string version) : this(id, version == null ? null : VersionUtility.ParseVersionSpec(version)) {}

    public ExternalDependency(string id, IVersionSpec version) {
      Version = version;
      Id = id;
    }

    public override string ToString() {
      if (Version != null) {
        return string.Format("{0}@{1}", Id, Version);
      }
      return Id;
    }
  }
}