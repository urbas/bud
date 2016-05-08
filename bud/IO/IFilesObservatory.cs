using System;

namespace Bud.IO {
  public interface IFilesObservatory {
    /// <param name="dir">the directory in which to watch files.</param>
    /// <param name="fileFilter">a filter such as <c>*</c>, <c>*.cs</c>, or <c>src/*.java</c>,
    /// or even <c>myFile.js</c>.</param>
    /// <param name="includeSubfolders">will watch files also in subfolders if set to <c>true</c>.
    /// Otherwise, only the top folder will be watched.</param>
    /// <returns>an observable stream of files that have changed.</returns>
    IObservable<string> CreateObserver(string dir,
                                       string fileFilter,
                                       bool includeSubfolders);
  }
}