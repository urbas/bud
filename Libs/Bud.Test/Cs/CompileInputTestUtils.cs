namespace Bud.Cs {
  public static class CompileInputTestUtils {
    public static CompileInput ToCompileInput(string sourceFile = null,
                                              CompileOutput dependency = null,
                                              string assembly = null)
      => new CompileInput(sourceFile == null ? new string[] {} : new[] {sourceFile},
                          dependency == null ? new CompileOutput[] {} : new[] {dependency},
                          assembly == null ? new string[] {} : new[] {assembly});
  }
}