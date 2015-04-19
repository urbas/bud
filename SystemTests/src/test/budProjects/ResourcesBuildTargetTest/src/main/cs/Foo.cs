using System.IO;

namespace Foo {
  public class Foo {
    public static void Main(string[] args) {
      PrintResourceContents("Foo.TestResourceFile.txt");
      PrintResourceContents("Foo.TestResourceFile2.txt");
    }

    private static void PrintResourceContents(string resourceFileName) {
      using (var resourceFile = typeof(Foo).Assembly.GetManifestResourceStream(resourceFileName)) {
        using (var resourceReader = new StreamReader(resourceFile)) {
          System.Console.Write(resourceReader.ReadToEnd());
        }
      }
    }
  }
}