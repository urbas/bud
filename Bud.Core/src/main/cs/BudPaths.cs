using System;
using System.IO;

namespace Bud {
  public static class BudPaths {
    public const string BudDirName = ".bud";
    public static readonly string GlobalConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), BudDirName);
  }
}