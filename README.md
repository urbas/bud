# Bud

Bud is a build tool. The word _Bud_ stands for _Build without the 'ill'_.

## How to perform a release?

Chocolatey:

- Didn't work... [One time]: Install the API key (has to be done in administrative console): `choco apikey -source https://chocolatey.org/ -k <your-key-here>`

- Change the version in the `chocolatey/bud.nuspec` file.

- Change the version in the `chocolatey/tools/chocolateyInstall.ps1` file.

- Invoke `bud project/bud/main/cs/dist`.

- Create a zip of the contents of the `bud\.bud\output\main\cs\dist` folder. The name of the zip file should be `bud-x.x.x.zip`

- Place the zip into `/home/budpage/production-budpage/shared/public/packages`.

- Go to the folder `chocolatey` and run `cpack` there.

- Didn't work. Upload manually for now on chocolatey. Now push the package with `choco push bud.x.x.x.nupkg`

## TODO

- Bud must be able to run tests.

- Bud has to be able to run executable projects via `bud run` or `bud run project/My.Project/main/cs/run`.

- Bud needs a concept of "macros". A macro can take parameters, reload or temporarily modify settings and then run a sequence of tasks. For example, `bud release` would take in the version of the next release, change the version run-time, publish packages, commit the new version to a file, and return the new settings.

- Bud must have an interactive and non-interactive command-line mode.

## Build structure

### Projects

A build consists of multiple projects.

Key: `project/:projectId`.  

### Project configuration

Each project has a number of configured properties, such as the base directory.

Key: `Project/baseDir`.

### Build targets

A project contains multiple build targets. A build target is defined by two keys: the build scope and the language.

Key: `Project/:scope/:language`.

Examples: `project/foo/main/cs`, `project/foo/test/cs`, `project/foo/main/java`.

## Global tasks

### `build`

Builds all build targets in all projects.

### `test`

Runs tests for all projects and all languages.