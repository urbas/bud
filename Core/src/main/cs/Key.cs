using System;
using System.Text;

namespace Bud {
  public class Key : MarshalByRefObject {
    public static readonly Key Root = new Key("Root", null, "The root scope. There can be only one!");
    public const char KeySeparator = '/';
    private static readonly char[] KeySplitter = {KeySeparator};
    public readonly Key Parent;
    public readonly string Id;
    public readonly string Description;
    private readonly int depth;
    private readonly int hash;

    public Key(string id, string description = null) : this(id, null, description) {}

    public Key(string id, Key parent, string description = null) {
      Parent = parent;
      Id = id;
      Description = description ?? string.Empty;
      depth = parent == null ? 1 : (parent.depth + 1);
      unchecked {
        hash = (parent == null ? -1498327287 : parent.hash) ^ Id.GetHashCode();
      }
    }

    public bool IsRoot {
      get { return Equals(this, Root); }
    }

    public bool IsAbsolute {
      get { return Equals(this, Root) || (Parent != null && Parent.IsAbsolute); }
    }

    public Key In(Key parent) {
      return Concat(parent, this);
    }

    public static Key Concat(Key parentKey, Key childKey) {
      if (parentKey == null) {
        return childKey;
      }
      if (childKey == null) {
        return parentKey;
      }
      if (!childKey.IsAbsolute) {
        return new Key(childKey.Id, Concat(parentKey, childKey.Parent), childKey.Description);
      }
      if (parentKey.IsRoot) {
        return childKey;
      }
      throw new ArgumentException("Cannot add a parent to an absolute key.");
    }

    public static Key Parse(string key) {
      if (string.IsNullOrEmpty(key)) {
        throw new ArgumentException("Could not parse an empty string. An empty string is not a valid key.");
      }
      var keyIdChain = key.Split(KeySplitter, StringSplitOptions.RemoveEmptyEntries);
      Key parsedKey = key[0] == KeySeparator ? Root : null;
      for (int index = 0; index < keyIdChain.Length; index++) {
        parsedKey = new Key(keyIdChain[index], parsedKey);
      }
      return parsedKey;
    }

    public bool Equals(Key otherKey) {
      return Equals(this, otherKey);
    }

    public static bool Equals(Key thisKey, Key otherKey) {
      if (ReferenceEquals(thisKey, otherKey)) {
        return true;
      }
      if (thisKey.depth != otherKey.depth) {
        return false;
      }
      return thisKey.Id.Equals(otherKey.Id) && Equals(thisKey.Parent, otherKey.Parent);
    }

    public override bool Equals(object other) {
      if (other == null) {
        return false;
      }
      if (!(other is Key)) {
        return false;
      }
      return Equals((Key) other);
    }

    public override int GetHashCode() {
      return hash;
    }

    public override string ToString() {
      return IsRoot ? KeySeparator.ToString() : PrependAsString(new StringBuilder(), Parent).Append(Id).ToString();
    }

    private static StringBuilder PrependAsString(StringBuilder stringBuilder, Key parent) {
      if (parent == null) {
        return stringBuilder;
      }
      var prepended = parent.IsRoot ? stringBuilder : PrependAsString(stringBuilder, parent.Parent).Append(parent.Id);
      return prepended.Append(KeySeparator);
    }

    public bool IdsEqual(Key otherKey) {
      return Id.Equals(otherKey.Id);
    }
  }
}