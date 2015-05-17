# Bud

> - __Latest version__: [0.4.0](https://github.com/urbas/bud/releases/tag/v0.4.0)
> - [__Documentation__](http://bud.urbas.si/Docs/Guide)
> - __Portable distribution__: [bud-0.4.0.zip](https://dl.dropboxusercontent.com/u/9516950/bud/bud-0.4.0.zip)
> - __Windows installation__: [Chocolatey](https://chocolatey.org/packages/bud/0.4.0)
> - __Ubuntu installation__: [bud_0.4.0_i386.deb](https://dl.dropboxusercontent.com/u/9516950/bud/bud_0.4.0_i386.deb)

Bud is a project automation tool for your C# projects. Bud downloads your projects' dependencies,
builds your projects, tests them, deploys them, releases them, and automates any other project-related tasks you desire.

Bud lets you write automation scripts and build configuration in pure C#. This way you can use the full power of your IDE to write your project automation.

## Quick-start

1.  Install Bud (use the links at the top).

1.  Create a folder for your project and navigate into it.

1.  Place some sources into the `src/main/cs` folder. Including a class with the `Main` method. Here's an example:

    ```csharp
    public class Application {
      public static void Main(string[] args) {
        System.Console.WriteLine("Hello, quick-start World!");
      }
    }
    ```

1.  Let Bud run your application:

    ```bash
    bud run
    ```

1.  Generate MsBuild project files:

    ```bash
    bud generateSolution
    ```

    Bud will list the files it generated. Open the one with the `.sln` extension.