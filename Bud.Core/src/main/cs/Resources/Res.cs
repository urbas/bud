using Bud.Build;

namespace Bud.Resources {
  public static class Res {
    public static Plugin Main(params Setup[] setups) => new ResourcesBuildTarget(BuildKeys.Main, setups);
    public static Plugin Test(params Setup[] setups) => new ResourcesBuildTarget(BuildKeys.Test, setups);
  }
}