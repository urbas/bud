## Upcoming release

- __CLI__ (new feature): Bud can now run in interactive mode. The interactive mode supports some rudamentary tab-completion and a simple history of commands.

- __CLI__ (breaking change): Bud will now run in interactive mode by default.

- __API__ (breaking change): Macro functions now take `IBuildContext` as the first parameter (instead of `BuildContext`). Also, the build context interface has been stripped down.

## 0.3.0

- __New feature__: Introduced the `@watch` macro. This macro watches sources in the `src` folder and evaluates given tasks whenever the sources change. Example of usage:


    ```language-bash
    bud @watch test
    ```

    The example above will run test whenever a source file changes.

- __CLI__ (breaking change): Removed the '--' command-line separator. You can now invoke macros with parameters like this: `bud @myMacro -v --foo bar`.

- __Improvement__: Output from spawned processes is now read asynchronously. Output will be printed during the execution instead of after the process terminates.

## 0.2.0

- __API-breaking change__: Key definitions from `CSharpKeys` have been moved to `Cs`.

- __New feature__: One can now pass custom arguments to NUnit via the key `Bud.CSharp.NUnitPlugin.NUnitArgs` in the `Cs.Test()` build target. For example:

    ```language-csharp
    new Project("Foo.Project", baseDir, Cs.Test(
      NUnitPlugin.NUnitArgs.Modify(list => list.Add("/noshadow"))
    ))
    ```

- __New feature__: You can now omit project's base directory. It will use `root/Project.Name` as the default. Here's an example:

    ```language-csharp
    new Project("Foo.Project", Cs.Exe())
    ```

- __New feature__: Added the `-v` command-line flag. This flag tells Bud to print its version and exit immediately after.

- __New feature__: Added the `-s` command-line flag. This flag tells Bud to print stack traces when exceptions occur.

- __New feature__: Commands can now be separated from the flags with `--`. This allows the usage of command-line options in macros. For example, the command `bud -jq -- @myMacro -v --foo bar` will pass parameters `-v`, `--foo`, and `bar` to the macro. Without `--`, option `-v` would be passed to `bud` (which would print the version and exit).

- __New feature__: You can now add macros to the build definition. See [documentation](http://bud.urbas.si/Docs/Guide#Macros) on how to use them.

- __Improvement__: Build definition projects file `./bud/src/main/cs/.bud.csproj` now includes assembly references that come with Bud: `System.Collections.Immutable`, `Newtonsoft.Json`, `CommandLine`, `NuGet.Core`, and `Antlr4.StringTemplate`.

## 0.1.2

- __New feature__: Added the `-q` command-line flag. It makes Bud suppress all logs.

- __New feature__: Added the `-j` command-line flag. It makes Bud return a JSON representation of the evaluated config or task. Example of usage:

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

- __API change__: Renamed `Settings.Do`, `Settings.Globally` and `Settings.In` to `Settings.Add`, `Settings.AddGlobally` and `Settings.AddIn`.

- __API change__: Ported `Setup` delegates to the `Plugin` class.

## 0.0.2

- __New feature__: Build-definition plugins are now downloaded from NuGet repositories.

- __New feature__: Added support for plugin development.

- __New feature__: Bud now parses command line arguments.

- __New feature__: Added the `generateSolution` task. It generates `.sln` and `.csproj` files.

## 0.0.1

Initial release.
