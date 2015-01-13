using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Bud.Plugins.Build;
using Bud.Plugins.BuildLoading;

namespace Bud {

  public static class BudAssemblies {
    public static IEnumerable<string> GetBudAssembliesLocations() {
      return GetBudAssemblies().Select(assembly => assembly.Location);
    }

    public static IEnumerable<Assembly> GetBudAssemblies() {
      return AppDomain.CurrentDomain.GetAssemblies()
        .Where(assembly => assembly.GetName().Name.StartsWith("Bud."));
    }

    public static Assembly GetBudCoreAssembly() {
      return GetBudAssemblies()
        .First(assembly => assembly.GetName().Name.EndsWith(".Core"));
    }
  }
  
}
