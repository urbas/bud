using System;
using System.Collections.Immutable;

namespace Bud.Keys {
  internal static class KeyCreator {
    public static T Define<T>(Key parentKey, T childKey, IKeyFactory<T> keyFactory) where T : Key {
      if (parentKey == null) {
        return childKey;
      }
      if (childKey == null) {
        return keyFactory.Define(parentKey.Path, parentKey.Description);
      }
      if (!childKey.IsAbsolute) {
        return keyFactory.Define(parentKey.Path.AddRange(childKey.Path), childKey.Description);
      }
      if (parentKey.IsRoot) {
        return childKey;
      }
      throw new ArgumentException("Cannot add a parent to an absolute key.");
    }

    public static T Define<T>(ImmutableList<string> path, string description, IKeyFactory<T> keyFactory) {
      return keyFactory.Define(path, description);
    }

    public static T Define<T>(Key parentKey, string id, string description, IKeyFactory<T> keyFactory) {
      return Define(parentKey.Path.Add(id), description, keyFactory);
    }

    public static T Define<T>(string id, string description, IKeyFactory<T> keyFactory) {
      return Define(ImmutableList.Create(id), description, keyFactory);
    }
  }
}