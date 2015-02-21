using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Bud {
  // TODO: Store keys in a pool to speed up equality comparisons.
  public class Key : MarshalByRefObject, IKey {
    public const string RootId = "root";
    public static readonly Key Root = Define(RootId, "The root scope. There can be only one!");
    public const char KeySeparator = '/';
    private static readonly char[] KeySplitter = {KeySeparator};
    private readonly int cachedHash;
    private Key cachedParent;
    private string cachedPathString;

    public static Key Define(string id, string description = null) {
      return Define(ImmutableList.Create(id), description);
    }

    public static Key Define(Key parentKey, Key childKey) {
      if (parentKey == null) {
        return childKey;
      }
      if (childKey == null) {
        return Define(parentKey.Path, parentKey.Description);
      }
      if (!childKey.IsAbsolute) {
        return Define(parentKey.Path.AddRange(childKey.Path), childKey.Description);
      }
      if (parentKey.IsRoot) {
        return childKey;
      }
      throw new ArgumentException("Cannot add a parent to an absolute key.");
    }

    public static Key Define(Key parentKey, string id, string description = null) {
      return Define(parentKey.Path.Add(id), description);
    }

    public static Key Define(ImmutableList<string> path, string description = null) {
      return new Key(path, description);
    }

    protected internal Key(ImmutableList<string> path, string description = null) {
      Id = path[path.Count - 1];
      Path = path;
      cachedHash = CalculateHashCode(path);
      Description = description;
    }

    public string Id { get; }

    public string Description { get; }

    public ImmutableList<string> Path { get; }

    public bool IsRoot {
      get { return PathDepth == 1 && IsAbsolute; }
    }

    public bool IsAbsolute {
      get { return RootId.Equals(Path[0]); }
    }

    public int PathDepth {
      get { return Path.Count; }
    }

    public Key Parent {
      get {
        if (cachedParent == null) {
          cachedParent = PathDepth > 1 ? new Key(Path.GetRange(0, PathDepth - 1)) : null;
        }
        return cachedParent;
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

    public static bool Equals(Key thisKey, IKey otherKey) {
      if (ReferenceEquals(thisKey, otherKey)) {
        return true;
      }
      if (thisKey.GetHashCode() != otherKey.GetHashCode()) {
        return false;
      }
      if (thisKey.PathDepth != otherKey.PathDepth) {
        return false;
      }
      return ArePathsEqual(thisKey.Path, otherKey.Path);
    }

    public override int GetHashCode() {
      return cachedHash;
    }

    public override string ToString() {
      if (cachedPathString == null) {
        cachedPathString = BuildPathString();
      }
      return cachedPathString;
    }

    private string BuildPathString() {
      var sb = new StringBuilder();
      var pathEnumerator = Path.GetEnumerator();
      if (IsAbsolute) {
        pathEnumerator.MoveNext();
        sb.Append(KeySeparator);
      }
      if (pathEnumerator.MoveNext()) {
        sb.Append(pathEnumerator.Current);
        while (pathEnumerator.MoveNext()) {
          sb.Append(KeySeparator).Append(pathEnumerator.Current);
        }
      }
      var asString = sb.ToString();
      return asString;
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
      return ConcatenatePath(parent == null ? null : parent.Path, newComponent);
    }

    private static int CreateNextHashCode(int parentHash, string nextPathComponent) {
      unchecked {
        return parentHash ^ nextPathComponent.GetHashCode();
      }
    }

    private static int CalculateHashCode(ImmutableList<string> pathToAppend) {
      return pathToAppend.Aggregate(0, CreateNextHashCode);
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