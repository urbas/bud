using System.IO;

namespace Bud.IO {
  public static class MemoryStreams {
    public static MemoryStream ToMemoryStream(string contentString) {
      var stream = new MemoryStream();
      var writer = new StreamWriter(stream);
      writer.Write(contentString);
      writer.Flush();
      stream.Seek(0, SeekOrigin.Begin);
      return stream;
    }
  }
}