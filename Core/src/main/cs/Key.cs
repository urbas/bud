using System;
using System.Collections.Immutable;
using System.Text;

namespace Bud {
  // TODO: Store keys in a pool to speed up equality comparisons.
  public class Key : MarshalByRefObject, IKey {
    public const string RootId = "root";
    public static readonly Key Root = Define(RootId, "The root scope. There can be only one!");
    public const char KeySeparator = '/';
    private static readonly char[] KeySplitter = {KeySeparator};
    private Key CachedParent;

    public static Key Define(string id, string description = null) {
      return Define(ImmutableList.Create(id), description);
    }

    public static Key Define(Key parentKey, Key childKey) {
      if (parentKey == null) {
        return childKey;
      }
      if (childKey == null) {
        return Define(parentKey.PathComponents, parentKey.Description);
      }
      if (!childKey.IsAbsolute) {
        return Define(parentKey.PathComponents.AddRange(childKey.PathComponents), childKey.Description);
      }
      if (parentKey.IsRoot) {
        return childKey;
      }
      throw new ArgumentException("Cannot add a parent to an absolute key.");
    }

    public static Key Define(Key parentKey, string id, string description = null) {
      return Define(parentKey.PathComponents.Add(id), description);
    }

    private static Key Define(ImmutableList<string> path, string description = null) {
      return new Key(path, description);
    }

    protected internal Key(ImmutableList<string> path, string description = null) {
      Id = path[path.Count - 1];
      PathComponents = path;
      Description = description;
      Path = BuildPathString(PathComponents);
    }

    public string Id { get; private set; }

    public string Description { get; private set; }

    public string Path { get; private set; }

    public ImmutableList<string> PathComponents { get; private set; }

    public bool IsRoot {
      get { return PathDepth == 1 && IsAbsolute; }
    }

    public bool IsAbsolute {
      get { return RootId.Equals(PathComponents[0]); }
    }

    public int PathDepth {
      get { return PathComponents.Count; }
    }

    public Key Parent {
      get {
        if (CachedParent == null) {
          CachedParent = PathDepth > 1 ? new Key(PathComponents.GetRange(0, PathDepth - 1)) : null;
        }
        return CachedParent;
      }
    }

    public static Key Parse(string key) {
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
      return Define(parsedPath.ToImmutable());
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

    private static bool ArePathsEqual(ImmutableList<string> pathA, ImmutableList<string> pathB) {
      // TODO: Investigate whether we can efficiently perform a reverse-traversal comparison. This is an optimisation, as it is more likely that paths will differ closer to the end.
      var iterA = pathA.GetEnumerator();
      var iterB = pathB.GetEnumerator();
      while (true) {
        var aHadNext = iterA.MoveNext();
        var bHadNext = iterB.MoveNext();
        if (aHadNext ^ bHadNext) {
          return false;
        }
        if (aHadNext) {
          if (!iterA.Current.Equals(iterB.Current)) {
            return false;
          }
        } else {
          return true;
        }
      }
    }
  }
}