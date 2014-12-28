using System;

namespace Bud {
  public interface IValueDefinition {
    object Evaluate(EvaluationContext context);
  }
}

