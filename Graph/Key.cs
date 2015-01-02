using System.IO;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using System.Text;
using System;

namespace Bud {
  public class Key : MarshalByRefObject {
    public static readonly Key Global = new Key("Global", null);
    public const char ScopeSeparator = ':';
    private static readonly char[] ScopeSplitter = new char[] {ScopeSeparator};
    public readonly Key Parent;
    public readonly string Id;
    private readonly int depth;
    private readonly int hash;

    public Key(string id) : this(id, Global) {}

    public Key(string id, Key parent) {
      Parent = parent ?? this;
      Id = id;
      depth = parent == null ? 1 : (parent.depth + 1);
      unchecked {
        hash = (parent == null ? -1498327287 : parent.hash) ^ Id.GetHashCode();
      }
    }

    public bool IsGlobal { get { return this == Parent; } }

    public Key In(Key parent) {
      return Concat(parent, this);
    }

    public static Key Concat(Key parentScope, Key childScope) {
      if (parentScope.IsGlobal) {
        return childScope;
      } else if (childScope.IsGlobal) {
        return parentScope;
      } else {
        return new Key(childScope.Id, Concat(parentScope, childScope.Parent));
      }
    }

    public static Key Parse(string scope) {
      if (string.IsNullOrEmpty(scope)) {
        throw new ArgumentException("Could not parse an empty string. An empty string is not a valid scope.");
      }
      var scopeIdChain = scope.Split(ScopeSplitter, StringSplitOptions.RemoveEmptyEntries);
      Key parsedScope = Global;
      foreach (var scopeId in scopeIdChain) {
        parsedScope = new Key(scopeId, parsedScope);
      }
      return parsedScope;
    }

    public bool Equals(Key otherScope) {
      if (ReferenceEquals(this, otherScope)) {
        return true;
      }
      if (depth != otherScope.depth) {
        return false;
      }
      return Id.Equals(otherScope.Id) && Parent.Equals(otherScope.Parent);
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
      return IsGlobal ? Id : AppendAsString(new StringBuilder()).ToString();
    }

    private StringBuilder AppendAsString(StringBuilder stringBuilder) {
      return IsGlobal ? stringBuilder : Parent.AppendAsString(stringBuilder).Append(':').Append(Id);
    }
  }
}

