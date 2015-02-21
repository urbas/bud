using System;
using System.Collections.Immutable;

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

    internal Key(string path, string description = null) {
      Description = description;
      Path = path;
    }

    public string Description { get; private set; }

    public Key Leaf {
      get {
        if (CachedLeaf == null) {
          if (IsRoot) {
            CachedLeaf = this;
          } else {
            var leafId = KeyPathUtils.ExtractIdFromPath(Path);
            CachedLeaf = leafId.Equals(Path) ? this : new Key(leafId, Description);
          }
        }
        return CachedLeaf;
      }
    }

    public string Path { get; private set; }

    public string Id {
      get { return CachedId ?? (CachedId = KeyPathUtils.ExtractIdFromPath(Path)); }
    }

    public ImmutableList<string> PathComponents {
      get { return CachedPathComponents ?? (CachedPathComponents = KeyPathUtils.ToPathComponents(Path)); }
    }

    public bool IsRoot {
      get { return KeyPathUtils.IsRootPath(Path); }
    }

    public bool IsAbsolute {
      get { return KeyPathUtils.IsAbsolutePath(Path); }
    }

    public Key Parent {
      get {
        if (CachedParent == null) {
          var parentPath = KeyPathUtils.ExtractParentPath(Path);
          CachedParent = KeyPathUtils.IsRootPath(parentPath) ? Root : new Key(parentPath, KeyPathUtils.ExtractIdFromPath(parentPath));
        }
        return CachedParent;
      }
    }


    public static Key Parse(string key) {
      return KeyPathUtils.IsRootPath(key) ? Root : new Key(key, KeyPathUtils.ExtractIdFromPath(key));
    }

    public override bool Equals(object other) {
      if (other == null) {
        return false;
      }
      var otherKey = other as IKey;
      if (otherKey == null) {
        return false;
      }
      return Equals(otherKey);
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