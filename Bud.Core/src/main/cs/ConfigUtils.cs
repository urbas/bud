using System.Threading.Tasks;

namespace Bud {
  public static class ConfigUtils {
    public static Task Evaluate(this IConfig config, string key) => config.Evaluate(Key.Parse(key));
  }
}