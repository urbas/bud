## 0.1.3 (next release)

- __New feature__: One can now pass custom arguments to NUnit via the key `Bud.CSharp.NUnitTestTargetPlugin.NUnitArgumentsKey` in the `Cs.Test()` build target. For example:

    ```language-csharp
    new Project("Foo.Project", baseDir, Cs.Test(
        Cs.Dependency("Bud.Test"),
        NUnitTestTargetPlugin.NUnitArgumentsKey.Modify(list => list.Add("/noshadow"))
    ))
    ```

## 0.1.2 (latest release)

- __New feature__: Added the `-q` flag, which suppresses logs.

- __New feature__: Added the `-j` flag, which returns a JSON representation of the evaluated config or task. Example of usage:

    ```
    bud.exe -qj projects
    ```

- __New feature__: Bud now exits with exit code 1 when anything fails (such as failing tests). On success, Bud will exit with code 0.

## 0.1.1

- __Bug fix__: Bud did not embed resources from the `Res.Main()` build target into the main C# build target.

## 0.1.0

- __Bug fix__: Task dependencies execution was not parallel.

## 0.0.3

- __New feature__: added the `run` task (executes executable projects).

- __New feature__: added the `test` task (runs NUnit tests).

- __API change__: Renamed `Settings.Do`, `Settings.Globally` and `Settings.In` to `Settings.Add`, `Settings.AddGlobally and `Settings.AddIn`.

- __API change__: Ported `Setup` delegates to the `Plugin` class.

## 0.0.2

- __New feature__: Build-definition plugins are now downloaded from NuGet repositories.

- __New feature__: Added support for plugin development.

- __New feature__: Bud now parses command line arguments.

- __New feature__: Added the `generateSolution` task. It generates `.sln` and `.csproj` files.

## 0.0.1

Initial release.
