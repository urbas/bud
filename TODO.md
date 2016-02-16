# TODO List

- [Ongoing] I want to install Bud from Chocolatey.

    - Cannot run from the distribution zip due to a problem with Immutable Collections:

        ```
        Unhandled Exception: System.TypeInitializationException: The type initializer for 'Bud.Cli.BuildTool' threw an exception. ---> System.IO.FileLoadException: Could not load file or assembly 'System.Collections.Immutable, Version=1.1.38.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference. (Exception from HRESULT: 0x80131040) ---> System.IO.FileLoadException: Could not load file or assembly 'System.Collections.Immutable, Version=1.1.37.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference. (Exception from HRESULT: 0x80131040)
        --- End of inner exception stack trace ---
        at Bud.Cli.BuildTool..cctor()
        --- End of inner exception stack trace ---
        at Bud.Cli.BuildTool.Main(String[] args)
        ```

- Add ability to reference packages and assemblies in buid configuration scripts.

- Improved CLI Logging. Example: invoking `Bud.Test/Compile`

        21:15:45.000> Bud/Packages/Resolve
        21:15:48.000< Bud/Packages/Resolve [3000ms]
        21:15:48.000> Bud/Compile
        21:15:48.250< Bud/Compile [250ms]
        21:15:48.250> Bud.Test/Packages/Resolve
        21:15:49.000< Bud.Test/Packages/Resolve [750ms]
        21:15:49.000> Bud.Test/Compile
        21:15:49.300< Bud.Test/Compile [300ms]

- I want to use Bud in Visual Source.

    - Bud should communicate to Visual Source through HTTP. The protocol:

        - GET for `IObservable` keys takes 1, serializes to JSON, and returns it.

        - GET for `Task` keys waits and return JSON-serialized result.

        - GET for everything else simply returns a JSON-serialized result.

        - WebSockets are available only for `IObservable` keys.

- Compile projects in parallel.

- I want to test my projects.

- Generate `csproj` files for projects.

- I want to generate MSBuild solution and project files for my projects.

- I want some help in `README.md`. After 1 minute, I should be able to build my project.

- I want to define a generic build which doesn't rebuild if sources are up-to-date with outputs.

    [Partial solution] Consider using `HashBasedCaching.GetLinesOrCache`.

- Use less than 10ms to start a simple build.

- I want to clean my projects without cleaning the resolved packages.

  - [Workaround] You can define a top-level task that does what you want:

        Clean.Init(c => {
          c.TryGet("Bud"/Clean);
          c.TryGet("Bud.Test"/Clean);
          return Unit.Default;
        })

  - [Proposal 1] We can introduce an 'Alias(Key, ...)' function to be used like this: `Alias(Clean, "Bud/Clean", "Bud.Test/Clean")`. One can then define arbitrary aliases that suit the needs of the developer.

  - [Proposal 2] We could allow wildcard keys, such as `bud */Clean` and `bud **/Clean`. (Partially implemented in git history: 195bde314a1e717143dac800a733efbcadcd3b86)

- Alias `bud Command` to `bud **/Command`. Do not alias absolute paths like `bud /Command` or wildcarded paths like `bud */Command` and `bud a/**/b`.

    - [Workaround] define a top-level command that you want to alias.