# Roadmap

Here are the next top-priority goals for Bud.

- Continuous task invocation

- Release plugin

- Integration with Kythe

- Integration with Jenkins and TeamCity

- Integration with Visual Studio

- Interactive mode

- Compilation with Roslyn

- Incremental compilation

- Packages for Linux and OS X

## Continuous task invocation

Bud must automatically re-invoke a selected task when any file in a watched directory changes.

__Proposed syntax__:

```
bud @watch build
```

## Release plugin

The release plugin should:

- publish libraries to NuGet

- publish full applications to Chocolatey

- publish release notes

- bump versions

## Integration with Kythe

Plug Bud into Kythe.

## Integration with Jenkins and TeamCity

Make Bud easily usable from CI servers.

## Integration with Visual Studio

Bud should provide project definition to Visual Studio and hook into Visual Studio's build flow.

## Interactive mode

Bud should be able to start once and then listen for commands.

__Proposed command-line syntax__:

If implemented with macros:

```language-bash
bud @interactive
```

if implemented otherwise:

```language-bash
bud -i
```

## Compilation with Roslyn

Roslyn should enable us to perhaps do incremental compilation.

## Incremental compilation

Only recompile the files that actually changed and their dependencies.

## Packages for Linux and OS X

Bud should be easily installable on both Linux and OS X.
