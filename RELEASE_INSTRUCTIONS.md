1. Install the API key (has to be done in administrative console): `choco apiKey -k <your-api-key-here> -source https://chocolatey.org/`

2. Change the version in the `chocolatey/bud.nuspec` file.

3. Change the version in the `chocolatey/tools/chocolateyInstall.ps1` file.

4. Invoke `bud project/bud/main/cs/dist`.

5. Create a zip of the contents of the `bud\.bud\output\main\cs\dist` folder. The name of the zip file should be `bud-x.x.x.zip`

6. Place the zip into `/home/budpage/production-budpage/shared/public/packages`.

7. Go to the folder `chocolatey` and run `cpack` there.

8. Upload manually for now on chocolatey. Now push the package with `choco push bud.x.x.x.nupkg`
