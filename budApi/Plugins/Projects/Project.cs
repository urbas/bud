using System.IO;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using Bud.SettingsConstruction.Ops;

namespace Bud.Plugins {

  public static class Project {
    public static SettingKey New(string id) {
      return new SettingKey(id);
    }
  }

}

