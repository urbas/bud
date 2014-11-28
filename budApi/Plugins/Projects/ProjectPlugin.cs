using System.IO;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using Bud.SettingsConstruction.Ops;

namespace Bud.Plugins {

  public class Project {
    public readonly string Id;
    public readonly string BaseDir;

    public Project(string id, string baseDir) {
      this.BaseDir = baseDir;
      this.Id = id;
    }
  }

}

