using System;

namespace Bud {
  public sealed class Unit {

    public static readonly Unit Instance = new Unit();

    private Unit() { }
  }
}

