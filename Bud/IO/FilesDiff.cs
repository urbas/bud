using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public class FilesDiff {
    public ImmutableHashSet<string> AddedFiles { get; }
    public ImmutableHashSet<string> RemovedFiles { get; }
    public ImmutableHashSet<string> ChangedFiles { get; }
    public ImmutableHashSet<string> AllFiles { get; }

    private FilesDiff(ImmutableHashSet<string> addedFiles, ImmutableHashSet<string> removedFiles, ImmutableHashSet<string> changedFiles, ImmutableHashSet<string> allFiles) {
      AddedFiles = addedFiles;
      RemovedFiles = removedFiles;
      ChangedFiles = changedFiles;
      AllFiles = allFiles;
    }

    public static IObservable<FilesDiff> DoDiffing(IObservable<FilesUpdate> filesObservable) 
      => DoDiffing(filesObservable, FileTimestamps.Instance);

    public static IObservable<FilesDiff> DoDiffing(IObservable<FilesUpdate> filesObservable, IFileTimestamps fileTimestamps) {
      var oldFiles = ImmutableHashSet<string>.Empty;
      var oldFilesWithTimestamps = ImmutableDictionary<string, DateTime>.Empty;
      return filesObservable.Select(files => {
        var newFiles = ImmutableHashSet.CreateRange(files.Enumerate());
        var newFilesWithTimestamps = ImmutableDictionary.CreateRange(newFiles.Select(s => new KeyValuePair<string, DateTime>(s, fileTimestamps.GetTimestamp(s))));
        var addedFiles = newFiles.Except(oldFiles);
        var removedFiles = oldFiles.Except(newFiles);
        var changedFiles = newFiles.Intersect(oldFiles).Where(s => newFilesWithTimestamps[s] > oldFilesWithTimestamps[s]).ToImmutableHashSet();
        oldFiles = newFiles;
        oldFilesWithTimestamps = newFilesWithTimestamps;
        return new FilesDiff(addedFiles, removedFiles, changedFiles, newFiles);
      });
    }
  }
}