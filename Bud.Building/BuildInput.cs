using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Bud.Building {
  static internal class BuildInput {
    public static readonly IReadOnlyList<string> EmptyInputFiles = new ReadOnlyCollection<string>(new string[0]);
  }
}