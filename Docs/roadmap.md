# Roadmap

Here are the next top-priority goals for Bud.

- [x] Continuous task invocation

- [ ] Release plugin
 
- [ ] Integration with Kythe

- [ ] Integration with Jenkins and TeamCity

- [ ] Integration with Visual Studio

- [x] Interactive mode

- [ ] Compilation with Roslyn

- [ ] Incremental compilation

- [ ] Packages for Linux and OS X

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

## Compilation with Roslyn

Roslyn should enable us to perhaps do incremental compilation.

## Incremental compilation

Only recompile the files that actually changed and their dependencies.

## Packages for Linux and OS X

Bud should be easily installable on both Linux and OS X.
