using System.IO;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using Bud.SettingsConstruction.Ops;

namespace Bud.Plugins {

  public class Project : SettingKey {
    public Project(string id) : base(id) {}

    public override string ToString() {
      return string.Format("Project({0})", Id);
    }
  }

}

