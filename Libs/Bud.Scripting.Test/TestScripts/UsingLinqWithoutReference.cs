﻿using System.IO;
using System.Linq;

namespace Bud.Scripting.TestScripts {
  public class UsingLinqWithoutReference {
    public static void Main(string[] args)
      => File.WriteAllText("foo", string.Join(" ", args.Select(s => s.ToUpper())));
  }
}