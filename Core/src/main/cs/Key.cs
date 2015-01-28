using System;
using System.Text;

namespace Bud {
  public class Key : MarshalByRefObject {
    public static readonly Key Global = new Key("Global", null, "The root scope. There can be only one!");
    public const char KeySeparator = '/';
    private static readonly char[] KeySplitter = {KeySeparator};
    public readonly Key Parent;
    public readonly string Id;
    public readonly string Description;
    private readonly int depth;
    private readonly int hash;

    public Key(string id, string description = null) : this(id, Global, description) {}

    public Key(string id, Key parent, string description = null) {
      Parent = parent ?? this;
      Id = id;
      Description = description ?? string.Empty;
      depth = parent == null ? 1 : (parent.depth + 1);
      unchecked {
        hash = (parent == null ? -1498327287 : parent.hash) ^ Id.GetHashCode();
      }
    }

    public bool IsRoot { get { return this == Parent; } }

    public Key In(Key parent) {
      return Concat(parent, this);
    }

    public static Key Concat(Key parentKey, Key childKey) {
      if (parentKey.IsRoot) {
        return childKey;
      }
      if (childKey.IsRoot) {
        return parentKey;
      }
      return new Key(childKey.Id, Concat(parentKey, childKey.Parent));
    }

    public static Key Parse(string key) {
      if (string.IsNullOrEmpty(key)) {
        throw new ArgumentException("Could not parse an empty string. An empty string is not a valid key.");
      }
      var keyIdChain = key.Split(KeySplitter, StringSplitOptions.RemoveEmptyEntries);
      Key parsedKey = Global;
      foreach (var keyId in keyIdChain) {
        parsedKey = new Key(keyId, parsedKey);
      }
      return parsedKey;
    }

    public bool Equals(Key otherKey) {
      if (ReferenceEquals(this, otherKey)) {
        return true;
      }
      if (depth != otherKey.depth) {
        return false;
      }
      return Id.Equals(otherKey.Id) && Parent.Equals(otherKey.Parent);
    }

    public override bool Equals(object other) {
      if (other == null) {
        return false;
      }
      if (!(other is Key)) {
        return false;
      }
      return Equals((Key)other);
    }

    public override int GetHashCode() {
      return hash;
    }

    public override string ToString() {
      return IsRoot ? KeySeparator.ToString() : AppendAsString(new StringBuilder()).ToString();
    }

    private StringBuilder AppendAsString(StringBuilder stringBuilder) {
      if (IsRoot) {
        return stringBuilder;
      }

      if (!Parent.IsRoot) {
        Parent.AppendAsString(stringBuilder).Append(KeySeparator);
      }

      return stringBuilder.Append(Id);
    }
  }
}

