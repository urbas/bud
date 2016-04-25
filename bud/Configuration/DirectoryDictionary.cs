using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Util;
using Bud.V1;
using static Bud.Util.Option;
using static Bud.V1.Keys;

namespace Bud.Configuration {
  /// <summary>
  ///   The keys of this dictionary are slash-separated paths. This
  ///   dictionary also maintains a <see cref="CurrentDir" />,
  ///   which is used to resolve relative paths on insertion and on
  ///   lookup.
  /// </summary>
  /// <remarks>
  ///   This class is not thread safe.
  /// </remarks>
  public class DirectoryDictionary<T> {
    /// <summary>
    ///   The dictionary to which key-value pairs are added.
    /// </summary>
    private readonly IDictionary<string, T> dictionary;

    /// <summary>
    ///   The path relative to which key paths are interpreted
    ///   when looking up key-value pairs or inserting new key-value pairs.
    /// </summary>
    public IImmutableList<string> CurrentDir { get; }

    /// <summary>
    ///   Creates a directory dictionary which inserts values
    ///   at paths interpreted from the given <see cref="CurrentDir" />.
    /// </summary>
    /// <param name="dictionary">
    ///   the underlying dictionary to which all key-value
    ///   pairs are inserted.
    /// </param>
    /// <param name="currentDir">
    ///   the directory relative to which key paths are interpreted.
    /// </param>
    public DirectoryDictionary(IDictionary<string, T> dictionary,
                               IImmutableList<string> currentDir) {
      this.dictionary = dictionary;
      CurrentDir = currentDir;
    }

    /// <summary>
    ///   Interprets the <paramref name="path" /> from the <see cref="CurrentDir" />
    ///   to an absolute path. The absolute path is then used to lookup the
    ///   value in this directory.
    /// </summary>
    /// <param name="path">
    ///   a relative or an absolute slash-separated path.
    /// </param>
    /// <returns>
    ///   the value at the given path.
    /// </returns>
    /// <remarks>
    ///   The method <see cref="Keys.ToFullPath" /> is used to
    ///   interpret the given <paramref name="path" />
    ///   relative to the <see cref="CurrentDir" />.
    /// </remarks>
    public Option<T> TryGet(string path) {
      T value;
      if (dictionary.TryGetValue(ToFullPath(path, CurrentDir), out value)) {
        return Some(value);
      }
      return None<T>();
    }

    /// <summary>
    ///   Interprets the <paramref name="path" /> relative to the
    ///   <see cref="CurrentDir" /> and converts it to an absolute path.
    ///   The resulting absolute path will be used as
    ///   the key. The key and value are inserted into the dictionary.
    /// </summary>
    /// <param name="path">
    ///   relative or absolute slash-separated path.
    /// </param>
    /// <param name="value">
    ///   the value to be set on the given path.
    /// </param>
    /// <returns>
    ///   this instance with the added value at the given path.
    /// </returns>
    public DirectoryDictionary<T> Set(string path, T value) {
      var fullPath = ToFullPath(path, CurrentDir);
      dictionary[fullPath] = value;
      return this;
    }

    /// <param name="subDirectory">
    ///   The name of the directory to add to the <see cref="CurrentDir" />.
    /// </param>
    /// <returns>
    ///   A new directory dictionary with a changed current directory.
    ///   The new current directory is the old one with the new sub-directory
    ///   appended.
    /// </returns>
    /// <remarks>
    ///   This method will not modify this
    ///   <see cref="DirectoryDictionary{T}" /> instance.
    /// </remarks>
    public DirectoryDictionary<T> In(string subDirectory)
      => new DirectoryDictionary<T>(dictionary, CurrentDir.Add(subDirectory));

    /// <summary>
    ///   Same as <see cref="In(string)" /> but with multiple
    ///   levels of sub-directories.
    /// </summary>
    public DirectoryDictionary<T> In(IEnumerable<string> path)
      => new DirectoryDictionary<T>(dictionary, CurrentDir.AddRange(path));
  }
}