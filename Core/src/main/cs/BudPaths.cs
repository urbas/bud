using System;
using System.IO;

namespace Bud {
  public static class BudPaths {
    public static readonly string GlobalConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".bud");
  }
}