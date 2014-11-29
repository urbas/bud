using System;

namespace Bud {
  public interface IValueDefinition { }
  public interface IValueDefinition<out T> : IValueDefinition {
    T Evaluate(BuildConfiguration buildConfiguration);
  }
}

