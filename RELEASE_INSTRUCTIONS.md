__Preparation__:

- Install the API key (has to be done in administrative console): `choco apiKey -k <your-api-key-here> -source https://chocolatey.org/`

__Release__:

1. Edit release notes.

1. Invoke the following: 

    ```
    bud @performRelease -v x.y.z
    ```


## Notes

### Ubuntu packaging

How to create a Ubuntu package:

1. Install package development tools:

    ```bash
    sudo apt-get install devscripts build-essential lintian dh-make
    ```

1. Run the `DevelopmentUtils/UbuntuPackage/create-package.sh` script.
