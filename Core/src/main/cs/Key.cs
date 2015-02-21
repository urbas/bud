using System;
using System.Collections.Immutable;
using System.Text;

namespace Bud {
  // TODO: Store keys in a pool to speed up equality comparisons.
  public class Key : MarshalByRefObject, IKey {
    public const string RootId = "root";
    public const char KeySeparator = '/';
    public static readonly string KeySeparatorAsString = KeySeparator.ToString();
    private static readonly char[] KeySplitter = {KeySeparator};
    public static readonly Key Root = new Key(KeySeparator.ToString(), "The root scope. There can be only one!");
    private Key CachedParent;
    private ImmutableList<string> CachedPathComponents;
    private string CachedId;

    public static Key Define(string id, string description = null) {
      return new Key(id, description);
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
      return new Key(KeyPathUtils.JoinPath(parentKey.Path, id), description);
    }

    internal Key(string path, string description = null) {
      Description = description;
      Path = path;
    }

    public string Description { get; private set; }

    public string Path { get; private set; }

    public string Id {
      get { return CachedId ?? (CachedId = KeyPathUtils.ExtractIdFromPath(Path)); }
    }

    public ImmutableList<string> PathComponents {
      get {
        if (CachedPathComponents == null) {
          CachedPathComponents = ToPathComponents(Path);
        }
        return CachedPathComponents;
      }
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

    private static ImmutableList<string> ToPathComponents(string key) {
      if (string.IsNullOrEmpty(key)) {
        throw new ArgumentException("Could not parse an empty string. An empty string is not a valid key.");
      }
      var keyIdChain = key.Split(KeySplitter, StringSplitOptions.RemoveEmptyEntries);
      var parsedPath = ImmutableList.CreateBuilder<string>();
      if (key[0] == KeySeparator) {
        parsedPath.Add(RootId);
      }
      for (int index = 0; index < keyIdChain.Length; index++) {
        parsedPath.Add(keyIdChain[index]);
      }
      var pathComponents = parsedPath.ToImmutable();
      return pathComponents;
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

    private static string BuildPathString(ImmutableList<string> pathComponents) {
      var pathEnumerator = pathComponents.GetEnumerator();
      if (pathEnumerator.MoveNext()) {
        var sb = new StringBuilder();
        if (RootId.Equals(pathEnumerator.Current)) {
          sb.Append(KeySeparator);
          if (pathEnumerator.MoveNext()) {
            sb.Append(pathEnumerator.Current);
          }
        } else {
          sb.Append(pathEnumerator.Current);
        }
        while (pathEnumerator.MoveNext()) {
          sb.Append(KeySeparator).Append(pathEnumerator.Current);
        }
        return sb.ToString();
      }
      throw new Exception("Cannot convert the list of path components to a string. The list of path components must not be empty.");
    }

    public bool IdsEqual(IKey otherKey) {
      return Id.Equals(otherKey.Id);
    }

    public static Key operator /(Key parent, Key child) {
      return Define(parent, child);
    }

    public static Key operator /(Key parent, string id) {
      return Define(parent, id);
    }

    protected static ImmutableList<string> ConcatenatePath(ImmutableList<string> parentPath, string newComponent) {
      return parentPath == null ? ImmutableList.Create(newComponent) : parentPath.Add(newComponent);
    }

    protected static ImmutableList<string> ConcatenatePath(Key parent, string newComponent) {
      return ConcatenatePath(parent == null ? null : parent.PathComponents, newComponent);
    }
  }
}