Preparation:

- Install the API key (has to be done in administrative console): `choco apiKey -k <your-api-key-here> -source https://chocolatey.org/`

Release steps:

1. Set the release version and prepare the release: 

    ```
    VER=0.1.2 && \
    sed -r "s/(<version>).+(<\/version>)/\1$VER\2/" -i chocolatey/bud.nuspec && \
    sed -r "s/(bud-).+?(.zip)/\1$VER\2/" -i chocolatey/tools/chocolateyInstall.ps1 && \
    git commit -am "Release $VER." && \
    git tag v$VER
    ```

1. Invoke `bud project/bud/main/cs/dist`.

1. Create a zip of the contents of the `bud\.bud\output\main\cs\dist` folder. The name of the zip file should be `bud-x.x.x.zip`

1. Place the zip into `/home/budpage/production-budpage/shared/public/packages`.

1. Go to the folder `chocolatey` and run `cpack` there.

1. Upload manually for now on chocolatey. Now push the package with `choco push bud.x.x.x.nupkg`
