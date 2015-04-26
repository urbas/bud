using System;
using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Bud {
  // TODO: Store keys in a pool to speed up equality comparisons.
  public class Key : MarshalByRefObject, IKey {
    public const string RootId = "root";
    public const char KeySeparator = '/';
    public static readonly string KeySeparatorAsString = KeySeparator.ToString();
    public static readonly Key Root = new Key(KeySeparator.ToString(), "The root scope. There can be only one!");
    private Key CachedParent;
    private ImmutableList<string> CachedPathComponents;
    private string CachedId;
    private Key CachedLeaf;

    [JsonConstructor]
    internal Key(string path, string description = null) {
      Description = description;
      Path = KeyPathUtils.NormalizePath(path);
    }

    public static Key Define(string id, string description = null) {
      return new Key(KeyPathUtils.ParseId(id), description);
    }

    public static Key Define(Key parentKey, Key childKey) {
      if (parentKey == null) {
        return childKey;
      }
      if (childKey == null) {
        return parentKey;
      }
      if (!childKey.IsAbsolute) {
        return new Key(KeyPathUtils.JoinPath(parentKey.Path, childKey.Path), childKey.Description);
      }
      if (parentKey.IsRoot) {
        return childKey;
      }
      throw new ArgumentException("Cannot add a parent to an absolute key.");
    }

    public static Key Define(Key parentKey, string id, string description = null) {
      return new Key(KeyPathUtils.JoinPath(parentKey.Path, KeyPathUtils.ParseId(id)), description);
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; }

    [JsonIgnore]
    public Key Leaf {
      get {
        if (CachedLeaf == null) {
          if (IsRoot) {
            CachedLeaf = this;
          } else {
            CachedLeaf = Id.Equals(Path) ? this : new Key(Id, Description);
          }
        }
        return CachedLeaf;
      }
    }

    public string Path { get; }

    [JsonIgnore]
    public string Id => CachedId ?? (CachedId = KeyPathUtils.ExtractIdFromPath(Path));

    [JsonIgnore]
    public ImmutableList<string> PathComponents => CachedPathComponents ?? (CachedPathComponents = KeyPathUtils.ToPathComponents(Path));

    [JsonIgnore]
    public bool IsRoot => KeyPathUtils.IsRootPath(Path);

    [JsonIgnore]
    public bool IsAbsolute => KeyPathUtils.IsAbsolutePath(Path);

    [JsonIgnore]
    public Key Parent {
      get {
        if (CachedParent == null) {
          var parentPath = KeyPathUtils.ExtractParentPath(Path);
          CachedParent = KeyPathUtils.IsRootPath(parentPath) ? Root : new Key(parentPath);
        }
        return CachedParent;
      }
    }


    public static Key Parse(string key) {
      return KeyPathUtils.IsRootPath(key) ? Root : new Key(key);
    }

    public override bool Equals(object other) {
      var otherKey = other as IKey;
      return otherKey != null && Equals(otherKey);
    }

    public bool Equals(IKey otherKey) {
      return Equals(this, otherKey);
    }

    public static bool Equals(IKey thisKey, IKey otherKey) {
      return thisKey.Path.Equals(otherKey.Path);
    }

    public override int GetHashCode() {
      return Path.GetHashCode();
    }

    public override string ToString() {
      return Path;
    }

    public static Key operator /(Key parent, Key child) {
      return Define(parent, child);
    }

    public static Key operator /(Key parent, string id) {
      return Define(parent, id);
    }
  }
}