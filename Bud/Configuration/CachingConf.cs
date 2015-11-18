namespace Bud.Configuration {
  public class CachingConf : IConf {
    private readonly ConfValueCache confValueCache;

    public CachingConf(ConfValueCalculator wrappedConf) {
      confValueCache = new ConfValueCache(new ValueCalcWrapper(this, wrappedConf));
    }

    public T Get<T>(Key<T> key) => confValueCache.Get(key.Relativize());

    private class ValueCalcWrapper : IConf {
      private readonly IConf conf;
      private readonly ConfValueCalculator valueCalculator;

      public ValueCalcWrapper(IConf conf, ConfValueCalculator valueCalculator) {
        this.conf = conf;
        this.valueCalculator = valueCalculator;
      }

      public T Get<T>(Key<T> key) => valueCalculator.Get(key, conf);
    }
  }
}