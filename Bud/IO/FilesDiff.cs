using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bud.IO {
  public class FilesDiff {
    public static readonly FilesDiff Empty = new FilesDiff(ImmutableHashSet<string>.Empty, ImmutableHashSet<string>.Empty, ImmutableHashSet<string>.Empty, ImmutableHashSet<string>.Empty, ImmutableDictionary<string, DateTime>.Empty);
    public ImmutableHashSet<string> AddedFiles { get; }
    public ImmutableHashSet<string> RemovedFiles { get; }
    public ImmutableHashSet<string> ChangedFiles { get; }
    public ImmutableHashSet<string> AllFiles { get; }
    public ImmutableDictionary<string, DateTime> FilesToTimestamps { get; }

    private FilesDiff(ImmutableHashSet<string> addedFiles, ImmutableHashSet<string> removedFiles, ImmutableHashSet<string> changedFiles, ImmutableHashSet<string> allFiles, ImmutableDictionary<string, DateTime> filesToTimestamps) {
      AddedFiles = addedFiles;
      RemovedFiles = removedFiles;
      ChangedFiles = changedFiles;
      AllFiles = allFiles;
      FilesToTimestamps = filesToTimestamps;
    }

    public static FilesDiff Create(IEnumerable<string> files, FilesDiff oldFileDiff) => Create(FileTimestamps.Instance, files, oldFileDiff);

    public static FilesDiff Create(IFileTimestamps fileTimestamps, IEnumerable<string> files, FilesDiff oldFilesDiff) {
      var newFiles = ImmutableHashSet.CreateRange(files);
      var newFilesToTimestamps = ImmutableDictionary.CreateRange(newFiles.Select(s => new KeyValuePair<string, DateTime>(s, fileTimestamps.GetTimestamp(s))));
      var addedFiles = newFiles.Except(oldFilesDiff.AllFiles);
      var removedFiles = oldFilesDiff.AllFiles.Except(newFiles);
      var changedFiles = newFiles.Intersect(oldFilesDiff.AllFiles).Where(s => newFilesToTimestamps[s] > oldFilesDiff.FilesToTimestamps[s]).ToImmutableHashSet();
      return new FilesDiff(addedFiles, removedFiles, changedFiles, newFiles, newFilesToTimestamps);
    }
  }
}